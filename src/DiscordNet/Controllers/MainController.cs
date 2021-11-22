using Discord.WebSocket;
using DiscordNet.Github;
using System;
using System.Threading.Tasks;

namespace DiscordNet
{
    public class MainController
    {
        public DiscordSocketClient Client;
        public IServiceProvider Services;

        public CommandHandler CommandHandler { get; private set; }
        public QueryHandler QueryHandler { get; private set; }
        public SheepHandler SheepHandler { get; private set; }
        public AutoPublishHandler PublishHandler { get; private set; }

        public readonly string Prefix = "<@274366085011079169> ";

        public MainController(DiscordSocketClient client, IServiceProvider services)
        {
            Client = client;
            Services = services;
            CommandHandler = new CommandHandler();
            SheepHandler = new SheepHandler();
            QueryHandler = new QueryHandler((GithubRest)services.GetService(typeof(GithubRest)));
            PublishHandler = new AutoPublishHandler();
        }

        public async Task InitializeEarlyAsync()
        {
            await CommandHandler.InitializeAsync(this, Services);
            await SheepHandler.InitializeAsync(this, Services);
            await PublishHandler.InitializeAsync(this, Services);
            QueryHandler.Initialize();
        }
    }
}
