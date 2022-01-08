using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNet
{
    public class CommandHandler
    {
        private CommandService _commandService;
        private InteractionService _interactionService;
        private DiscordSocketClient _client;
        private IServiceProvider _services;
        private MainController _mainHandler;
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromMinutes(3) });

        public async Task InitializeAsync(MainController MainHandler, IServiceProvider services)
        {
            _mainHandler = MainHandler;
            _client = services.GetService<DiscordSocketClient>();
            _commandService = new CommandService();
            _interactionService = new InteractionService(_client);
            _services = services;

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _commandService.Log += Log;
            _interactionService.Log += Log;

            _client.MessageReceived += HandleCommand;
            _client.MessageUpdated += HandleUpdate;

            _client.Ready += async () =>
            {
                await _interactionService.RegisterCommandsToGuildAsync(848176216011046962).ConfigureAwait(false);
            };

            _client.InteractionCreated += async (intr) =>
            {
                await _interactionService.ExecuteCommandAsync(new SocketInteractionContext(_client, intr), _services);
            };
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Exception);
            return Task.CompletedTask;
        }

        public Task Close()
        {
            _client.MessageReceived -= HandleCommand;
            return Task.CompletedTask;
        }

        private Task HandleUpdate(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            if (!(after is SocketUserMessage afterSocket))
                return Task.CompletedTask;
            _ = Task.Run(async () =>
            {
                ulong? id;
                if ((id = GetOurMessageIdFromCache(before.Id)) != null)
                {
                    if (!(await channel.GetMessageAsync(id.Value) is IUserMessage botMessage))
                        return;
                    int argPos = 0;
                    if (!afterSocket.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;
                    var reply = await BuildReply(afterSocket, after.Content.Substring(argPos));

                    if (reply.Item1 == null && reply.Item2 == null)
                        return;

                    await botMessage.ModifyAsync(x => { x.Content = reply.Item1; x.Embed = reply.Item2?.Build(); });
                }
            });
            return Task.CompletedTask;
        }

        public Task HandleCommand(SocketMessage parameterMessage)
        {
            var msg = parameterMessage as SocketUserMessage;
            if (msg == null || msg.Author.IsBot)
                return Task.CompletedTask;

            if (msg.Channel is ITextChannel textChannel && textChannel.GuildId != 848176216011046962)
                return Task.CompletedTask;

            int argPos = 0;
            if (!(msg.HasMentionPrefix(_client.CurrentUser, ref argPos) /*|| msg.HasStringPrefix(MainHandler.Prefix, ref argPos)*/)) return Task.CompletedTask;
            _ = HandleCommandAsync(msg, argPos);
            return Task.CompletedTask;
        }

        public async Task HandleCommandAsync(SocketUserMessage msg, int argPos)
        {
            var reply = await BuildReply(msg, msg.Content.Substring(argPos));
            if (reply.Item1 == null && reply.Item2 == null)
                return;
            IUserMessage message = null;
            try
            {
                message = await msg.Channel.SendMessageAsync(reply.Item1, embed: reply.Item2?.Build()).ConfigureAwait(false);
                AddCache(msg.Id, message.Id);
            }
            catch (Exception x)
            {
                
            }
            
        }

        private async Task<(string, EmbedBuilder)> BuildReply(IUserMessage msg, string message)
        {
            var context = new BotCommandContext(_client, _mainHandler, msg);
            var result = await _commandService.ExecuteAsync(context, message, _services);
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                return (result.ErrorReason, null);
            else if (!result.IsSuccess)
            {
                if (!_mainHandler.QueryHandler.IsReady())
                    return ("Loading cache, please wait a few moments before trying again.", null);
                else
                {
                    try
                    {
                        var tuple = await _mainHandler.QueryHandler.RunAsync(message);
                        return (tuple.Item1, tuple.Item2);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        return ("Uh-oh... I think some pipes have broken...", null);
                    }
                }
            }
            return (null, null);
        }

        public void AddCache(ulong userMessageId, ulong ourMessageId)
            => _cache.Set(userMessageId, ourMessageId, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

        public ulong? GetOurMessageIdFromCache(ulong messageId)
            => _cache.TryGetValue<ulong>(messageId, out ulong id) ? (ulong?)id : null;

        public async Task<EmbedBuilder> HelpEmbedBuilderAsync(ICommandContext context, string command = null)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.Author = new EmbedAuthorBuilder().WithName("Help:").WithIconUrl("http://i.imgur.com/VzDRjUn.png");
            StringBuilder sb = new StringBuilder();
            if (command == null)
            {
                foreach (Discord.Commands.ModuleInfo mi in _commandService.Modules.OrderBy(x => x.Name))
                    if (!mi.IsSubmodule)
                        if (mi.Name != "Help")
                        {
                            bool ok = true;
                            foreach (Discord.Commands.PreconditionAttribute precondition in mi.Preconditions)
                                if (!(await precondition.CheckPermissionsAsync(context, null, _services)).IsSuccess)
                                {
                                    ok = false;
                                    break;
                                }
                            if (ok)
                            {
                                var cmds = mi.Commands.ToList<object>();
                                cmds.AddRange(mi.Submodules);
                                for (int i = cmds.Count - 1; i >= 0; i--)
                                {
                                    object o = cmds[i];
                                    foreach (Discord.Commands.PreconditionAttribute precondition in ((o as CommandInfo)?.Preconditions ?? (o as Discord.Commands.ModuleInfo)?.Preconditions))
                                        if (!(await precondition.CheckPermissionsAsync(context, o as CommandInfo, _services)).IsSuccess)
                                            cmds.Remove(o);
                                }
                                if (cmds.Count != 0)
                                {
                                    var list = cmds.Select(x => $"{((x as CommandInfo)?.Name ?? (x as Discord.Commands.ModuleInfo)?.Name)}").OrderBy(x => x);
                                    sb.AppendLine($"**{mi.Name}:** {string.Join(", ", list)}");
                                }
                            }
                        }

                eb.AddField((x) =>
                {
                    x.IsInline = false;
                    x.Name = "Query help";
                    x.Value = $"Usage: {context.Client.CurrentUser.Mention} [query]";
                });
                eb.AddField((x) =>
                {
                    x.IsInline = true;
                    x.Name = "Keywords";
                    x.Value = "method, type, property,\nevent, in, list";
                });
                eb.AddField((x) =>
                {
                    x.IsInline = true;
                    x.Name = "Examples";
                    x.Value = "EmbedBuilder\n" +
                              "IGuildUser.Nickname\n" +
                              "ModifyAsync in IRole\n" +
                              "send message\n" +
                              "type Emote";
                });
                eb.Footer = new EmbedFooterBuilder().WithText("Note: (i) = Inherited");
                eb.Description = sb.ToString();
            }
            else
            {
                SearchResult sr = _commandService.Search(context, command);
                if (sr.IsSuccess)
                {
                    CommandMatch? cmd = null;
                    if (sr.Commands.Count == 1)
                        cmd = sr.Commands.First();
                    else
                    {
                        int lastIndex;
                        var find = sr.Commands.Where(x => x.Command.Aliases.First().Equals(command, StringComparison.OrdinalIgnoreCase));
                        if (find.Any())
                            cmd = find.First();
                        while (cmd == null && (lastIndex = command.LastIndexOf(' ')) != -1) //TODO: Maybe remove and say command not found?
                        {
                            find = sr.Commands.Where(x => x.Command.Aliases.First().Equals(command.Substring(0, lastIndex), StringComparison.OrdinalIgnoreCase));
                            if (find.Any())
                                cmd = find.First();
                            command = command.Substring(0, lastIndex);
                        }
                    }
                    if (cmd != null && (await cmd.Value.CheckPreconditionsAsync(context, _services)).IsSuccess)
                    {
                        eb.Author.Name = $"Help: {cmd.Value.Command.Aliases.First()}";
                        sb.Append($"Usage: {_mainHandler.Prefix}{cmd.Value.Command.Aliases.First()}");
                        if (cmd.Value.Command.Parameters.Count != 0)
                            sb.Append($" [{string.Join("] [", cmd.Value.Command.Parameters.Select(x => x.Name))}]");
                        if (!string.IsNullOrEmpty(cmd.Value.Command.Summary))
                            sb.Append($"\nSummary: {cmd.Value.Command.Summary}");
                        if (!string.IsNullOrEmpty(cmd.Value.Command.Remarks))
                            sb.Append($"\nRemarks: {cmd.Value.Command.Remarks}");
                        if (cmd.Value.Command.Aliases.Count != 1)
                            sb.Append($"\nAliases: {string.Join(", ", cmd.Value.Command.Aliases.Where(x => x != cmd.Value.Command.Aliases.First()))}");
                        eb.Description = sb.ToString();
                    }
                    else
                        eb.Description = $"Command '{command}' not found.";
                }
                else
                    eb.Description = $"Command '{command}' not found.";
            }
            return eb;
        }
    }
}
