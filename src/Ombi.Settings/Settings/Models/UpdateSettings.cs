namespace Ombi.Settings.Settings.Models
{
    public class UpdateSettings : Settings
    {
        public bool AutoUpdateEnabled { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ProcessName { get; set; }
        public bool UseScript { get; set; }
        public string ScriptLocation { get; set; }
        public string WindowsServiceName { get; set; }
        public bool WindowsService { get; set; }
        public bool TestMode { get; set; }
    }
}