using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Ombi.Core.Settings.Models
{
    public class LandingPageSettings : Ombi.Settings.Settings.Models.Settings
    {
        public bool Enabled { get; set; }
        
        [NotMapped]
        public bool NoticeEnabled => !string.IsNullOrEmpty(NoticeText);
        public string NoticeText { get; set; }

        public bool TimeLimit { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        [NotMapped]
        public bool Expired => EndDateTime > DateTime.Now;
    }
}