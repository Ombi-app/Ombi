namespace Ombi.Settings.Settings.Models
{
    public class UpdateSettings : Settings
    {
        public bool AutoUpdateEnabled { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ProcessName { get; set; }
    }
}