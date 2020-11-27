using System;
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
        public string description { get; set; }
        public DateTime timestamp => DateTime.Now;
        public string color { get; set; }
        public DiscordFooter footer { get; set; }
        public DiscordImage thumbnail { get; set; }
        public DiscordAuthor author { get; set; }
        public List<DiscordField> fields { get; set; }
    }

    public class DiscordFooter
    {
        public string text { get; set; }
    }

    public class DiscordAuthor
    {
        public string name { get; set; }
        public string url { get; set; }
        public string iconurl { get; set; }
    }

    public class DiscordField
    {
        public string name { get; set; }
        public string value { get; set; }
        public bool inline { get; set; }
    }

    public class DiscordImage
    {
        public string url { get; set; }
    }
}