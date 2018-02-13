using System.Collections.Generic;

namespace Ombi.Api.Radarr.Models
{
    public class MovieResponse
    {
        public string title { get; set; }
        public string sortTitle { get; set; }
        public double sizeOnDisk { get; set; }
        public string status { get; set; }
        public string overview { get; set; }
        public string inCinemas { get; set; }
        public string physicalRelease { get; set; }
        public List<Image> images { get; set; }
        public string website { get; set; }
        public bool downloaded { get; set; }
        public int year { get; set; }
        public bool hasFile { get; set; }
        public string youTubeTrailerId { get; set; }
        public string studio { get; set; }
        public string path { get; set; }
        public int profileId { get; set; }
        public string minimumAvailability { get; set; }
        public bool monitored { get; set; }
        public int runtime { get; set; }
        public string lastInfoSync { get; set; }
        public string cleanTitle { get; set; }
        public string imdbId { get; set; }
        public int tmdbId { get; set; }
        public string titleSlug { get; set; }
        public List<string> genres { get; set; }
        public List<object> tags { get; set; }
        public string added { get; set; }
        public Ratings ratings { get; set; }
        //public List<string> alternativeTitles { get; set; }
        public int qualityProfileId { get; set; }
        public int id { get; set; }
    }
}
