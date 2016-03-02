namespace RequestPlex.Core.SettingModels
{
    public class CouchPotatoSettings : Settings
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public bool Enabled { get; set; }
    }
}