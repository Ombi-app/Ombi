namespace Ombi.Settings.Settings.Models.Notifications
{
    public class WebhookSettings : Settings
    {
        public bool Enabled { get; set; }
        public string WebhookUrl { get; set; }
        public string ApplicationToken { get; set; }
    }
}