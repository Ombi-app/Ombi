using Ombi.Core.Settings.Models.External;

namespace Ombi.Settings.Settings.Models.External
{
    public class RadarrSettings : ExternalSettings
    {
        public bool Enabled { get; set; }
        public string ApiKey { get; set; }
        public string DefaultQualityProfile { get; set; }
        public string DefaultRootPath { get; set; }
        public string FullRootPath { get; set; }
        public bool AddOnly { get; set; }
    }
}