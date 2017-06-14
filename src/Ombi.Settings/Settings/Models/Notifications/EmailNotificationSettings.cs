namespace Ombi.Settings.Settings.Models.Notifications
{
    public class EmailNotificationSettings : Settings
    {
        public bool Enabled { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string Sender { get; set; }
        public string Username { get; set; }
        public bool Authentication { get; set; }
        public string AdminEmail { get; set; }
    }
}