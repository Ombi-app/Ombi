namespace Ombi.Settings.Settings.Models.Notifications
{
    public class MattermostNotificationSettings : Settings
    {
        public string WebhookUrl { get; set; }
        public string Channel { get; set; }
        public string Username { get; set; }
        public string IconUrl { get; set; }
        public bool   Enabled { get; set; }
    }
}