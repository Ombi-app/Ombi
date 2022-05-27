using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ombi.Api.Plex.Models
{
    public class Metadata
    {
        public string ratingKey { get; set; }
        public string key { get; set; }
        public string studio { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string contentRating { get; set; }
        public string summary { get; set; }
        public int index { get; set; }
        public int year { get; set; }
        public string thumb { get; set; }
        public string art { get; set; }
        public string banner { get; set; }
        public string theme { get; set; }
        public int leafCount { get; set; }
        public int viewedLeafCount { get; set; }
        public int childCount { get; set; }
        public Genre[] Genre { get; set; }
        public string primaryExtraKey { get; set; }
        public string parentRatingKey { get; set; }
        public string grandparentRatingKey { get; set; }
        public string guid { get; set; }
        public int librarySectionID { get; set; }
        public string librarySectionKey { get; set; }
        public string grandparentKey { get; set; }
        public string parentKey { get; set; }
        public string grandparentTitle { get; set; }
        public string parentTitle { get; set; }
        public int parentIndex { get; set; }
        public string parentThumb { get; set; }
        public string grandparentThumb { get; set; }
        public string grandparentArt { get; set; }
        public string grandparentTheme { get; set; }
        public string chapterSource { get; set; }
        public Medium[] Media { get; set; }


        [JsonProperty("guids")]      
        public List<PlexGuids> Guid { get; set; } = new List<PlexGuids>();
    }

    public class PlexGuids
    {
        public string Id { get; set; }
    }
}
