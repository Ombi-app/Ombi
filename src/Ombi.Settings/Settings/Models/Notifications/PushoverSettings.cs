using System;
using Newtonsoft.Json;

namespace Ombi.Settings.Settings.Models.Notifications
{
    public class PushoverSettings : Settings
    {
        public bool Enabled { get; set; }
        public string AccessToken { get; set; }
        public string UserToken { get; set; }
        public sbyte Priority { get; set; } = 0;
        public string Sound { get; set; } = "pushover";
    }
}