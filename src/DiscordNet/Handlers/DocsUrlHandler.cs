using System;

namespace DiscordNet
{
    public class DocsUrlHandler
    {
        public static string DocsBaseUrl { get; set; } = "https://discord-net-labs.com/";

        private string[] _docsUrls =
        {
            "https://docs.stillu.cc/",
            "https://discord.foxbot.me/latest/"
        };

        public bool CheckAvailability()
        {
            throw new NotImplementedException();
        }
    }
}
