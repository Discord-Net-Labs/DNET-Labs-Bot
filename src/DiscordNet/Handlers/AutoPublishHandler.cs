using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNet
{
    public class AutoPublishHandler
    {
        public static ulong[] AutoPublishChannels = new ulong[]
        {
            848177635002941491
        };

        DiscordSocketClient _client;
        public async Task InitializeAsync(MainController MainHandler, IServiceProvider services)
        {
            _client = services.GetService<DiscordSocketClient>();

            _client.MessageReceived += _client_MessageReceived;
        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            if (!AutoPublishChannels.Contains(arg.Channel.Id))
                return;

            if (arg is not SocketUserMessage userMsg)
                return;

            if (arg.Channel is not SocketNewsChannel news)
                return;

            await userMsg.CrosspostAsync();
        }
    }
}
