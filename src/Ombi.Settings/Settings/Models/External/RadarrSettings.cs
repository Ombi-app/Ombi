namespace Ombi.Settings.Settings.Models.External
{
    public class RadarrSettings : ExternalSettings
    {
        public bool Enabled { get; set; }
        public bool V3 { get; set; }
        public string ApiKey { get; set; }
        public string DefaultQualityProfile { get; set; }
        public string DefaultRootPath { get; set; }
        public bool AddOnly { get; set; }
        public string MinimumAvailability { get; set; }
        public bool ScanForAvailability { get; set; }
    }
}