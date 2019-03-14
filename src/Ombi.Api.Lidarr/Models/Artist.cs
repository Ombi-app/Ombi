using System;

namespace Ombi.Api.Lidarr.Models
{
    public class Artist
    {
        public string status { get; set; }
        public bool ended { get; set; }
        public string artistName { get; set; }
        public string foreignArtistId { get; set; }
        public int tadbId { get; set; }
        public int discogsId { get; set; }
        public object[] links { get; set; }
        public object[] images { get; set; }
        public int qualityProfileId { get; set; }
        public int languageProfileId { get; set; }
        public int metadataProfileId { get; set; }
        public bool albumFolder { get; set; }
        public bool monitored { get; set; }
        public object[] genres { get; set; }
        public object[] tags { get; set; }
        public DateTime added { get; set; }
        public Statistics statistics { get; set; }
    }
}