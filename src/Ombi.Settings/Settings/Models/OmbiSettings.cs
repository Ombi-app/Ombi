namespace Ombi.Settings.Settings.Models
{
    public class OmbiSettings : Models.Settings
    {
        //public string BaseUrl { get; set; }
        public bool CollectAnalyticData { get; set; }
        public bool Wizard { get; set; }
        public string ApiKey { get; set; }

    }
}