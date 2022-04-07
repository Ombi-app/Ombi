using System.Collections.Generic;

namespace Ombi.Api.Plex.Models
{
    public class PlexWatchlistMetadataContainer
    {
        public PlexWatchlistMetadata MediaContainer { get; set; }
    }


    public class PlexWatchlistMetadata
    {
        public int offset { get; set; }
        public int totalSize { get; set; }
        public string identifier { get; set; }
        public int size { get; set; }
        public WatchlistMetadata[] Metadata { get; set; }
    }

    public class WatchlistMetadata
    {
        public string guid { get; set; }
        public string key { get; set; }
        public string primaryExtraKey { get; set; }
        public string ratingKey { get; set; }
        public string type { get; set; }
        public string slug { get; set; }
        public string title { get; set; }
        public List<PlexGuids> Guid { get; set; } = new List<PlexGuids>();
    }
}