﻿using System;
using Newtonsoft.Json;

namespace Ombi.Core.SettingModels
{
    public sealed class DiscordNotificationSettings : NotificationSettings
    {
        public string WebhookUrl { get; set; }
        public string Username { get; set; }

        [JsonIgnore]
        public string WebookId => SplitWebUrl(4);

        [JsonIgnore]
        public string Token => SplitWebUrl(5);

        private string SplitWebUrl(int index)
        {
            if (!WebhookUrl.StartsWith("http", StringComparison.InvariantCulture))
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