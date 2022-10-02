using System;
using System.Net.Mime;

namespace Ombi.Api.Lidarr.Models
{
    public class ArtistAdd
    {
        public string artistName { get; set; }
        public string foreignArtistId { get; set; }
        public Link[] links { get; set; }
        public Image[] images { get; set; }
        public int qualityProfileId { get; set; }
        public int metadataProfileId { get; set; }
        public bool albumFolder { get; set; }
        public bool monitored { get; set; }
        public string cleanName { get; set; }
        public DateTime added { get; set; }
        public Addoptions addOptions { get; set; }
        public string rootFolderPath { get; set; }
    }

    public class Addoptions
    {
        public MonitorTypes monitor { get; set; }
        public bool monitored { get; set; }
        public bool searchForMissingAlbums { get; set; } // Only for Artists add
        public string[] AlbumsToMonitor { get; set; } // Uses the MusicBrainzAlbumId!
    }

    public enum MonitorTypes
    {
        All,
        Future,
        Missing,
        Existing,
        Latest,
        First,
        None,
        Unknown
    }
}
