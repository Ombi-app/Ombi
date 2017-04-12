namespace Ombi.Core.Settings.Models
{
    public class OmbiSettings : Settings
    {
        public int Port { get; set; }
        //public string BaseUrl { get; set; }
        public bool CollectAnalyticData { get; set; }
        public bool Wizard { get; set; }

        public string ApiKey { get; set; }
    }
}