using System;

namespace Ombi.Api.Lidarr.Models
{
    public class AlbumResponse
    {
        public string title { get; set; }
        public string disambiguation { get; set; }
        public int artistId { get; set; }
        public string foreignAlbumId { get; set; }
        public bool monitored { get; set; }
        public int profileId { get; set; }
        public int duration { get; set; }
        public string albumType { get; set; }
        public object[] secondaryTypes { get; set; }
        public int mediumCount { get; set; }
        public Ratings ratings { get; set; }
        public DateTime releaseDate { get; set; }
        public Currentrelease currentRelease { get; set; }
        public Release[] releases { get; set; }
        public object[] genres { get; set; }
        public Medium[] media { get; set; }
        public Image[] images { get; set; }
        public Statistics statistics { get; set; }
        public int id { get; set; }
    }
}