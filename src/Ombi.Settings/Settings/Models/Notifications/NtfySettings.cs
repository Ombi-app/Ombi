namespace Ombi.Settings.Settings.Models.Notifications
{
    public class NtfySettings : Settings
    {
        public bool Enabled { get; set; }
        public string BaseUrl { get; set; }
        public string AuthorizationHeader { get; set; }
        public string Topic { get; set; }
        public sbyte Priority { get; set; } = 4;
    }
}