using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
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
            848177635002941491,
            918142959491383296
        };

        private ConcurrentQueue<IUserMessage> _queue = new ConcurrentQueue<IUserMessage>();
        private TaskCompletionSource _queueSignal = new TaskCompletionSource();
        DiscordSocketClient _client;
        public async Task InitializeAsync(MainController MainHandler, IServiceProvider services)
        {
            _client = services.GetService<DiscordSocketClient>();

            _client.MessageReceived += _client_MessageReceived;

            _ = Task.Run(async () => await RunQueue());

            foreach(var id in AutoPublishChannels)
            {
                var guild = _client.GetGuild(848176216011046962);
                var channel = guild.GetTextChannel(id); ;

                var msgs = await channel.GetMessagesAsync(25).FlattenAsync();

                var unposted = msgs.Where(x => (x.Flags & MessageFlags.Crossposted) == 0);

                if (unposted.Any())
                    foreach (var msg in unposted)
                        if(msg is IUserMessage sum)
                            _queue.Enqueue(sum);
            }

            if (_queue.Any())
                _queueSignal?.SetResult();
        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            if (!AutoPublishChannels.Contains(arg.Channel.Id))
                return;

            if (arg is not SocketUserMessage userMsg)
                return;

            if (arg.Channel is not SocketNewsChannel)
                return;

            _queue.Enqueue(userMsg);
            try
            {
                _queueSignal?.SetResult();
            }
            catch { }
        }

        private async Task RunQueue()
        {
            while(true)
            {
                await _queueSignal.Task;

                while(_queue.TryDequeue(out var msg))
                {
                    try
                    {
                        await msg.CrosspostAsync();
                    }
                    catch { }
                }

                _queueSignal = new TaskCompletionSource();
            }
        }
    }
}
