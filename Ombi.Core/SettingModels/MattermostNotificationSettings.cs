using System;
using Newtonsoft.Json;

namespace Ombi.Core.SettingModels
{
    public sealed class MattermostNotificationSettings : NotificationSettings
    {
        public string WebhookUrl { get; set; }
        public string Channel { get; set; }
        public string Username { get; set; }
        public string IconUrl { get; set; }
    }
}