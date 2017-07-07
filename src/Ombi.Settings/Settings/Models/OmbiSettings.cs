namespace Ombi.Core.Settings.Models
{
    public class OmbiSettings : Ombi.Settings.Settings.Models.Settings
    {
        public int Port { get; set; }
        //public string BaseUrl { get; set; }
        public bool CollectAnalyticData { get; set; }
        public bool Wizard { get; set; }

        public string ExternalUrl { get; set; }
        public string ApiKey { get; set; }
    }
}