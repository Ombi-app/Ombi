namespace RequestPlex.Core.SettingModels
{
    public class SonarrSettings : Settings
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public bool Enabled { get; set; }
    }
}