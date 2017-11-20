namespace Ombi.Settings.Settings.Models
{
    public class JobSettings
    {
        public string EmbyContentSync { get; set; }
        public string SonarrSync { get; set; }
        public string RadarrSync { get; set; }
        public string PlexContentSync { get; set; }
        public string CouchPotatoSync { get; set; }
        public string AutomaticUpdater { get; set; }
        public string UserImporter { get; set; }
    }
}