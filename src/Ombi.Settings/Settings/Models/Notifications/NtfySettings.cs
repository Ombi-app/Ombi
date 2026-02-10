namespace Ombi.Settings.Settings.Models.Notifications
{
    public class NtfySettings : Settings
    {
        public bool Enabled { get; set; }
        public string BaseUrl { get; set; } = "https://ntfy.sh";
        public string Topic { get; set; }
        public string AuthorizationHeader { get; set; }
        public int Priority { get; set; } = 3;
    }
}
