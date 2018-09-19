using Ombi.Core.Settings.Models.External;

namespace Ombi.Settings.Settings.Models.External
{
    public class LidarrSettings : ExternalSettings
    {
        public bool Enabled { get; set; }
        public string ApiKey { get; set; }
        public string DefaultQualityProfile { get; set; }
        public string DefaultRootPath { get; set; }
        public bool AlbumFolder { get; set; }
        public int LanguageProfileId { get; set; }
        public int MetadataProfileId { get; set; }
        public bool AddOnly { get; set; }
    }
}