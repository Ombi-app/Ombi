using Newtonsoft.Json;

namespace Ombi.Api.MusicBrainz.Models.Lookup
{

    [JsonPluralName("release-groups")]
    public class ReleaseGroups
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "primary-type-ids")]
        public string PrimaryTypeId { get; set; }
        [JsonProperty(PropertyName = "disambiguation")]
        public string Disambiguation { get; set; }
        [JsonProperty(PropertyName = "secondary-types")]
        public string[] SecondaryTypes { get; set; }
        [JsonProperty(PropertyName = "primary-type")]
        public string PrimaryType { get; set; } // Album / Single / Live / EP
        [JsonProperty(PropertyName = "first-release-date")]
        public string FirstReleaseDate { get; set; }
        [JsonProperty(PropertyName = "secondary-type-ids")]
        public string[] SecondaryTypeIds { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; } // Release title
    }

}