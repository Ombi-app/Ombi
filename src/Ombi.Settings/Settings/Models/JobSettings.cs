namespace Ombi.Settings.Settings.Models
{
    public class JobSettings : Settings
    {
        public string EmbyContentSync { get; set; }
        public string JellyfinContentSync { get; set; }
        public string SonarrSync { get; set; }
        public string RadarrSync { get; set; }
        public string PlexContentSync { get; set; }
        public string PlexRecentlyAddedSync { get; set; }
        public string CouchPotatoSync { get; set; }
        public string AutomaticUpdater { get; set; }
        public string UserImporter { get; set; }
        public string SickRageSync { get; set; }
        public string Newsletter { get; set; }
        public string LidarrArtistSync { get; set; }
        public string IssuesPurge { get; set; }
        public string RetryRequests { get; set; }
        public string MediaDatabaseRefresh { get; set; }
        public string AutoDeleteRequests { get; set; }
    }
}
