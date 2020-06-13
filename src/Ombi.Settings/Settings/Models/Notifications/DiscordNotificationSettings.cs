using System;
using Newtonsoft.Json;

namespace Ombi.Settings.Settings.Models.Notifications
{
    public class DiscordNotificationSettings : Settings
    {
        public bool Enabled { get; set; }
        public string WebhookUrl { get; set; }
        public string Username { get; set; }
        public string Icon { get; set; }

        [JsonIgnore]
        public string WebHookId => SplitWebUrl(4);

        [JsonIgnore]
        public string Token => SplitWebUrl(5);

        private string SplitWebUrl(int index)
        {
            if (!WebhookUrl.StartsWith("http", StringComparison.CurrentCulture))
            {
                WebhookUrl = "https://" + WebhookUrl;
            }
            var split = WebhookUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            return split.Length < index
                ? string.Empty
                : split[index];
        }
    }
}