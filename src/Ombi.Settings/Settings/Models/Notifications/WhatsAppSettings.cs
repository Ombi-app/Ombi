namespace Ombi.Settings.Settings.Models.Notifications
{
    public class WhatsAppSettings : Settings
    {
        public bool Enabled { get; set; }
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string From { get; set; }
    }
}