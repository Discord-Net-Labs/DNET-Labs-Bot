using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DiscordNet
{
    public class SheepHandler
    {
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        public async Task InitializeAsync(MainController MainHandler, IServiceProvider services)
        {
            _client = services.GetService<DiscordSocketClient>();
            _services = services;

            _client.MessageReceived += HandleMessage;
        }

        public Task HandleMessage(SocketMessage parameterMessage)
        {
            ulong contributorsLoungeId = 878992667101503498;
            ulong contributorsVCTextId = 898571566563065866;
            var msg = parameterMessage as SocketUserMessage;
            if (msg == null || msg.Author.IsBot)
                return Task.CompletedTask;
            if (msg.Channel is ITextChannel textChannel && textChannel.GuildId != 848176216011046962)
                return Task.CompletedTask;
            if (msg.Author.Id == StaticGlobals.sheepedUserId && msg.Channel is ITextChannel textChannel1 && (textChannel1.Id == contributorsLoungeId || textChannel1.Id == contributorsVCTextId))
                _ = HandleSheepMessage(msg);

            return Task.CompletedTask;
        }

        public async Task HandleSheepMessage(SocketUserMessage msg)
        {
            string msgContent = msg.Content;
            Random r = new Random();
            string[] replies = new string[] { "Bah", "Baaaah", "Baaaaaaah!", "BAAAAH!" };
            await msg.DeleteAsync();
            int index = r.Next(replies.Length);
            await msg.Channel.SendMessageAsync($"{replies[index]}");
        }
    }
}
