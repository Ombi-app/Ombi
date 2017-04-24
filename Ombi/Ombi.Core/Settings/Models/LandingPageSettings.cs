using System;
using Newtonsoft.Json;

namespace Ombi.Core.Settings.Models
{
    public class LandingPageSettings : Settings
    {
        public bool Enabled { get; set; }
        public bool BeforeLogin { get; set; }

        [JsonIgnore]
        public bool AfterLogin => !BeforeLogin;

        public bool NoticeEnabled => !string.IsNullOrEmpty(NoticeText);
        public string NoticeText { get; set; }
        public string NoticeBackgroundColor { get; set; }

        public bool TimeLimit { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}