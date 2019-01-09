using System;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Api.Sonarr.Models
{

    public class SonarrSeries
    {
        public string title { get; set; }
        public Alternatetitle[] alternateTitles { get; set; }
        public string sortTitle { get; set; }
        public int seasonCount { get; set; }
        public int totalEpisodeCount { get; set; }
        public int episodeCount { get; set; }
        public int episodeFileCount { get; set; }
        public long sizeOnDisk { get; set; }
        public string status { get; set; }
        public string overview { get; set; }
        public DateTime previousAiring { get; set; }
        public string network { get; set; }
        public string airTime { get; set; }
        public Image[] images { get; set; }
        public Season[] seasons { get; set; }
        public int year { get; set; }
        public string path { get; set; }
        public int profileId { get; set; }
        public bool seasonFolder { get; set; }
        public bool monitored { get; set; }
        public bool useSceneNumbering { get; set; }
        public long runtime { get; set; }
        public int tvdbId { get; set; }
        public int tvRageId { get; set; }
        public int tvMazeId { get; set; }
        public DateTime firstAired { get; set; }
        public DateTime lastInfoSync { get; set; }
        public string seriesType { get; set; }
        public string cleanTitle { get; set; }
        public string imdbId { get; set; }
        public string titleSlug { get; set; }
        public string certification { get; set; }
        public string[] genres { get; set; }
        public object[] tags { get; set; }
        public DateTime added { get; set; }
        public Ratings ratings { get; set; }
        public int qualityProfileId { get; set; }
        public int languageProfileId { get; set; }
        public int id { get; set; }
        public DateTime nextAiring { get; set; }
    }

    public class Ratings
    {
        public int votes { get; set; }
        public float value { get; set; }
    }

    public class Alternatetitle
    {
        public string title { get; set; }
        public int sceneSeasonNumber { get; set; }
        public int seasonNumber { get; set; }
    }

    public class Image
    {
        public string coverType { get; set; }
        public string url { get; set; }
    }

    public class Season
    {
        public int seasonNumber { get; set; }
        public bool monitored { get; set; }
        public Statistics statistics { get; set; }
    }

    public class Statistics
    {
        public int episodeFileCount { get; set; }
        public int episodeCount { get; set; }
        public int totalEpisodeCount { get; set; }
        public long sizeOnDisk { get; set; }
        public long percentOfEpisodes { get; set; }
        public DateTime previousAiring { get; set; }
        public DateTime nextAiring { get; set; }
    }

}
