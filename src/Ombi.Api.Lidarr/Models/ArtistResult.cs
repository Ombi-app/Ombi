using System;

namespace Ombi.Api.Lidarr.Models
{

    public class ArtistResult
    {
        public string status { get; set; }
        public bool ended { get; set; }
        public DateTime lastInfoSync { get; set; }
        public string artistName { get; set; }
        public string foreignArtistId { get; set; }
        public int tadbId { get; set; }
        public int discogsId { get; set; }
        public string overview { get; set; }
        public string artistType { get; set; }
        public string disambiguation { get; set; }
        public Link[] links { get; set; }
        public Nextalbum nextAlbum { get; set; }
        public Image[] images { get; set; }
        public string path { get; set; }
        public int qualityProfileId { get; set; }
        public int languageProfileId { get; set; }
        public int metadataProfileId { get; set; }
        public bool albumFolder { get; set; }
        public bool monitored { get; set; }
        public object[] genres { get; set; }
        public string cleanName { get; set; }
        public string sortName { get; set; }
        public object[] tags { get; set; }
        public DateTime added { get; set; }
        public Ratings ratings { get; set; }
        public Statistics statistics { get; set; }
        public int id { get; set; }
    }

    public class Nextalbum
    {
        public string foreignAlbumId { get; set; }
        public int artistId { get; set; }
        public string title { get; set; }
        public string disambiguation { get; set; }
        public string cleanTitle { get; set; }
        public DateTime releaseDate { get; set; }
        public int profileId { get; set; }
        public int duration { get; set; }
        public bool monitored { get; set; }
        public object[] images { get; set; }
        public object[] genres { get; set; }
        public Medium[] media { get; set; }
        public DateTime lastInfoSync { get; set; }
        public DateTime added { get; set; }
        public string albumType { get; set; }
        public object[] secondaryTypes { get; set; }
        public Ratings ratings { get; set; }
        public Release[] releases { get; set; }
        public Currentrelease currentRelease { get; set; }
        public int id { get; set; }
    }

    public class Currentrelease
    {
        public string id { get; set; }
        public string title { get; set; }
        public DateTime releaseDate { get; set; }
        public int trackCount { get; set; }
        public int mediaCount { get; set; }
        public string disambiguation { get; set; }
        public string[] country { get; set; }
        public string format { get; set; }
        public string[] label { get; set; }
    }

    public class Medium
    {
        public int number { get; set; }
        public string name { get; set; }
        public string format { get; set; }
    }

    public class Release
    {
        public string id { get; set; }
        public string title { get; set; }
        public DateTime releaseDate { get; set; }
        public int trackCount { get; set; }
        public int mediaCount { get; set; }
        public string disambiguation { get; set; }
        public string[] country { get; set; }
        public string format { get; set; }
        public string[] label { get; set; }
    }
}