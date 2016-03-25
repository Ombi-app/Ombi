namespace PlexRequests.Core.SettingModels
{
    public class PushoverNotificationSettings : Settings
    {
        public bool Enabled { get; set; }
        public string AccessToken { get; set; }
        public string UserToken { get; set; }
    }
}