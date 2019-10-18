using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Api.Sonarr.Models
{
    public class NewSeries
    {
        public NewSeries()
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
        public string seriesType { get; set; }
        public int id { get; set; }
        public List<SonarrImage> images { get; set; }

        // V3 Property
        public int languageProfileId { get; set; }

        /// <summary>
        /// This is for us
        /// </summary>
        [JsonIgnore]
        public List<string> ErrorMessages { get; set; }

        public string Validate()
        {
            var sb = new StringBuilder();
            if(this.tvdbId == 0)
            {
                sb.AppendLine("TVDBID is missing");
            }
            if(string.IsNullOrEmpty(title))
            {
                sb.AppendLine("Title is missing");
            }
            if(qualityProfileId == 0)
            {
                sb.AppendLine("Quality ID is missing");
            }

            return sb.ToString();
        }

    }
    public class AddOptions
    {
        public bool ignoreEpisodesWithFiles { get; set; }
        public bool ignoreEpisodesWithoutFiles { get; set; }
        public bool searchForMissingEpisodes { get; set; }
    }

    public class SonarrImage
    {
        public string coverType { get; set; }
        public string url { get; set; }
    }

    public class SonarrError
    {
        public string propertyName { get; set; }
        public string errorMessage { get; set; }
        public object attemptedValue { get; set; }
    }
}
