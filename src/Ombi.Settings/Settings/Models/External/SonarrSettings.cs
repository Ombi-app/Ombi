namespace Ombi.Settings.Settings.Models.External
{
    public class SonarrSettings : ExternalSettings
    {
        public bool Enabled { get; set; }
        public string ApiKey { get; set; }
        public string QualityProfile { get; set; }
        public bool SeasonFolders { get; set; }
        /// <summary>
        /// This is the root path ID
        /// </summary>
        /// <value>
        /// The root path.
        /// </value>
        public string RootPath { get; set; }


        public string QualityProfileAnime { get; set; }
        public string RootPathAnime { get; set; }
        public bool AddOnly { get; set; }
        public bool V3 { get; set; }
        public int LanguageProfile { get; set; }
        public bool ScanForAvailability { get; set; }
    }
}