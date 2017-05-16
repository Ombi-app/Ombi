using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ombi.Api.Models.Sonarr
{
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
        public float percentOfEpisodes { get; set; }
        public DateTime previousAiring { get; set; }
    }

    public class SonarrAddSeries
    {
        public SonarrAddSeries()
        {
            images = new List<SonarrImage>();
        }
        public AddOptions addOptions { get; set; }
        public string title { get; set; }
        public List<Season> seasons { get; set; }
        public string rootFolderPath { get; set; }
        public int qualityProfileId { get; set; }
        public bool seasonFolder { get; set; }
        public bool monitored { get; set; }
        public int tvdbId { get; set; }
        public int tvRageId { get; set; }
        public string cleanTitle { get; set; }
        public string imdbId { get; set; }
        public string titleSlug { get; set; }
        public int id { get; set; }
        public List<SonarrImage> images { get; set; }
        [JsonIgnore]
        public List<string> ErrorMessages { get; set; }
    }

    public class AddOptions
    {
        public bool ignoreEpisodesWithFiles { get; set; }
        public bool ignoreEpisodesWithoutFiles { get; set; }
        public bool searchForMissingEpisodes { get; set; }
    }

    public class Addoptions
    {
        public bool searchForMissingEpisodes { get; set; }
        public bool ignoreEpisodesWithFiles { get; set; }
        public bool ignoreEpisodesWithoutFiles { get; set; }
    }

    public class SonarrImage
    {
        public string coverType { get; set; }
        public string url { get; set; }
    }
    
}
