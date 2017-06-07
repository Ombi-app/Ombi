namespace Ombi.Settings.Settings.Models.Notifications
{
    public sealed class EmailNotificationSettings : Settings
    {
        public bool Enabled { get; set; }
        public string EmailHost { get; set; }
        public string EmailPassword { get; set; }
        public int EmailPort { get; set; }
        public string EmailSender { get; set; }
        public string EmailUsername { get; set; }
        public bool Authentication { get; set; }
        public string RecipientEmail { get; set; }
    }
}