namespace Ombi.Core.Settings.Models.External
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
        public string FullRootPath { get; set; }

    }
}