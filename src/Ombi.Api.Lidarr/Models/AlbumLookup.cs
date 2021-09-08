using System;
using System.Collections.Generic;

namespace Ombi.Api.Lidarr.Models
{
    public class AlbumLookup
    {
        public string title { get; set; }
        public string status { get; set; }
        public string artistType { get; set; }
        public string overview { get; set; }
        public string disambiguation { get; set; }
        public List<LidarrLinks> links { get; set; }
        public int artistId { get; set; }
        public string foreignAlbumId { get; set; }
        public bool monitored { get; set; }
        public int profileId { get; set; }
        public int duration { get; set; }
        public string albumType { get; set; }
        public string[] secondaryTypes { get; set; }
        public int mediumCount { get; set; }
        public Ratings ratings { get; set; }
        public DateTime releaseDate { get; set; }
        //public object[] releases { get; set; }
        public object[] genres { get; set; }
        //public object[] media { get; set; }
        public Artist artist { get; set; }
        public Image[] images { get; set; }
        public string remoteCover { get; set; }
    }
}