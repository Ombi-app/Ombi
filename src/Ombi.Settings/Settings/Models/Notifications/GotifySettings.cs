namespace Ombi.Settings.Settings.Models.Notifications
{
    public class GotifySettings : Settings
    {
        public bool Enabled { get; set; }
        public string BaseUrl { get; set; }
        public string ApplicationToken { get; set; }
        public sbyte Priority { get; set; } = 4;
    }
}