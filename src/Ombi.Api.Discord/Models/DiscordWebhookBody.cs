using System.Collections.Generic;

namespace Ombi.Api.Discord.Models
{
    public class DiscordWebhookBody
    {
        public string content { get; set; }
        public string username { get; set; }
        public List<DiscordEmbeds> embeds { get; set; }
    }

    public class DiscordEmbeds
    {
        public string title { get; set; }
        public string type => "rich"; // Always rich or embedded content
        public string description { get; set; } // Don't really need to set this
        public DiscordImage image { get; set; }
    }

    public class DiscordImage
    {
        public string url { get; set; }
    }
}