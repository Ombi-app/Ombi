namespace PlexRequests.Core.SettingModels
{
    public class PushbulletNotificationSettings : Settings
    {
        public bool Enabled { get; set; }
        public string AccessToken { get; set; }
        public string DeviceIdentifier { get; set; }
    }
}