using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordNet
{
    public class SheepHandler
    {
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        private static Dictionary<ulong, DiscordWebhookClient> _webhooks = new Dictionary<ulong, DiscordWebhookClient>();

        public async Task InitializeAsync(MainController MainHandler, IServiceProvider services)
        {
            _client = services.GetService<DiscordSocketClient>();
            _services = services;

            _client.MessageReceived += HandleMessage;
        }

        public Task HandleMessage(SocketMessage parameterMessage)
        {
            var msg = parameterMessage as SocketUserMessage;
            if (msg == null || msg.Author.IsBot)
                return Task.CompletedTask;

            if (msg.Author is not SocketGuildUser sgu)
                return Task.CompletedTask;

            ulong contributorsLoungeId = 878992667101503498;
            ulong contributorsVCTextId = 898571566563065866;
           
            if (msg.Channel is ITextChannel textChannel && textChannel.GuildId != 848176216011046962)
                return Task.CompletedTask;


            if (sgu.Roles.Any(x => x.Id == 898731376214429716) && msg.Channel is ITextChannel textChannel1 && (textChannel1.Id == contributorsLoungeId || textChannel1.Id == contributorsVCTextId))
                _ = HandleSheepMessage(msg);

            return Task.CompletedTask;
        }

        public static async Task RemoveWebhooksAsync()
        {
            foreach(var hook in _webhooks)
            {
                await hook.Value.DeleteWebhookAsync();
                hook.Value.Dispose();
            }

            _webhooks.Clear();
        }

        public async Task HandleSheepMessage(SocketUserMessage msg)
        {
            await msg.DeleteAsync();

            var guildChannel = msg.Channel as SocketTextChannel;

            if (guildChannel == null)
                return;

            DiscordWebhookClient hook = null;

            if (_webhooks.ContainsKey(guildChannel.Id))
                hook = _webhooks[guildChannel.Id];
            else
            {
                var av = await new HttpClient().GetStreamAsync(msg.Author.GetAvatarUrl() ?? msg.Author.GetDefaultAvatarUrl());
                hook = new Discord.Webhook.DiscordWebhookClient(await guildChannel.CreateWebhookAsync($"{msg.Author}", av));
                _webhooks[guildChannel.Id] = hook;
            }

            Random r = new Random();
            var rng = r.Next(0, 20);
            string reply = $"B{"a".PadRight(rng, 'a')}h{((rng / 2 - 7) % 2 == 0 ? "!" : ".")}";
           
            await hook.SendMessageAsync($"{(rng % 2 == 0 ? reply.ToUpper() : reply)}");
        }
    }
}
