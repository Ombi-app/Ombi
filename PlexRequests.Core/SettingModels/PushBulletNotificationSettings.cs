namespace PlexRequests.Core.SettingModels
{
    public class PushbulletNotificationSettings : NotificationSettings
    {
        public bool Enabled { get; set; }
        public string AccessToken { get; set; }
        public string DeviceIdentifier { get; set; }
    }
}