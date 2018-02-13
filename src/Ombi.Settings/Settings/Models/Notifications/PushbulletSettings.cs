using System;
using Newtonsoft.Json;

namespace Ombi.Settings.Settings.Models.Notifications
{
    public class PushbulletSettings : Settings
    {
        public bool Enabled { get; set; }
        public string AccessToken { get; set; }
        public string ChannelTag { get; set; }
    }
}