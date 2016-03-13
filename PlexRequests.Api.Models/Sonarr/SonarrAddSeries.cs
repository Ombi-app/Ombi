using System.Collections.Generic;

namespace PlexRequests.Api.Models.Sonarr
{
    public class Season
    {
        public int seasonNumber { get; set; }
        public bool monitored { get; set; }
    }

    public class SonarrAddSeries
    {
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
    }

    public class AddOptions
    {
        public bool ignoreEpisodesWithFiles { get; set; }
        public bool ignoreEpisodesWithoutFiles { get; set; }
        public bool searchForMissingEpisodes { get; set; }
    }


    
}
