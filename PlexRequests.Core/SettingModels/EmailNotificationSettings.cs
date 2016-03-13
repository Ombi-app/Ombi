namespace PlexRequests.Core.SettingModels
{
    public class EmailNotificationSettings : Settings
    {
        public string EmailHost { get; set; }
        public int EmailPort { get; set; }
        public bool EmailAuthentication { get; set; }
        public string RecipientEmail { get; set; }
        public string EmailUsername { get; set; }
        public string EmailPassword { get; set; }
        public bool Enabled { get; set; }
    }
}