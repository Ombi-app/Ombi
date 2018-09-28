using System.Collections.Generic;

namespace Ombi.Settings.Settings.Models.Notifications
{
    public class NewsletterSettings : Settings
    {
        public bool DisableTv { get; set; }
        public bool DisableMovies { get; set; }
        public bool DisableMusic { get; set; }
        public bool Enabled { get; set; }
        public List<string> ExternalEmails { get; set; } = new List<string>();
    }
}