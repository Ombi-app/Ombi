namespace Ombi.Settings.Settings.Models.Notifications
{
    public class TwilioSettings : Settings
    {
        public WhatsAppSettings WhatsAppSettings { get; set; }
    }

    public class WhatsAppSettings
    {
        public bool Enabled { get; set; }
        public string From { get; set; }
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
    }
}