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
        public string url { get; set; }
        public DiscordAuthor author { get; set; }
        public DiscordFooter footer { get; set; }
        public int color => 14641434; // Default Ombi color
        public string type => "rich"; // Always rich or embedded content
        public string description { get; set; } // Don't really need to set this
        public DiscordImage image { get; set; }
        public List<DiscordField> fields { get; set; } 
        public DiscordImage thumbnail { get; set; }
    }

    public class DiscordImage
    {
        public string url { get; set; }
        // discord webhook does not allow width / height changes
    }

    public class DiscordAuthor
    {
        public string name { get; set; }
        public string url { get; set; }
        public string icon_url { get; set; }
    }

    public class DiscordFooter
    {
        public string text { get; set; }
    }

    public class DiscordField
    {
        public string name { get; set; }
        public string value { get; set; }
        public bool inline { get; set; }
    }
}