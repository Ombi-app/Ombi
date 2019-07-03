using Newtonsoft.Json;

namespace Ombi.Api.MusicBrainz.Models.Search
{

    [JsonPluralName("artists")]
    public class Artist
    {
        public string id { get; set; }
        public string type { get; set; }
        [JsonProperty(PropertyName = "type-id")]
        public string typeid { get; set; }
        public int score { get; set; }
        public string name { get; set; }
        [JsonProperty(PropertyName = "sort-name")]
        public string sortname { get; set; }
        public string country { get; set; }
        public Area area { get; set; }
        [JsonProperty(PropertyName = "begin-area")]
        public BeginArea beginarea { get; set; }
        [JsonProperty(PropertyName = "life-span")]
        public LifeSpan2 lifespan { get; set; }
        public Tag[] tags { get; set; }
        [JsonProperty(PropertyName = "isni-list")]
        public IsniList[] isnilist { get; set; }
        public string disambiguation { get; set; }
    }

    public class Area
    {
        public string id { get; set; }
        public string type { get; set; }
        [JsonProperty(PropertyName = "type-id")]
        public string typeid { get; set; }
        public string name { get; set; }

        [JsonProperty(PropertyName = "sort-name")]
        public string sortname { get; set; }
        [JsonProperty(PropertyName = "life-span")]
        public LifeSpan lifespan { get; set; }
    }

    public class LifeSpan
    {
        public object ended { get; set; }
    }

    public class BeginArea
    {
        public string id { get; set; }
        public string type { get; set; }
        [JsonProperty(PropertyName = "type-id")]
        public string typeid { get; set; }
        public string name { get; set; }
        [JsonProperty(PropertyName = "sort-name")]
        public string sortname { get; set; }
        [JsonProperty(PropertyName = "life-span")]
        public LifeSpan1 lifespan { get; set; }
    }

    public class LifeSpan1
    {
        public object ended { get; set; }
    }

    public class LifeSpan2
    {
        public string begin { get; set; }
        public object ended { get; set; }
    }

    public class Tag
    {
        public int count { get; set; }
        public string name { get; set; }
    }

    public class IsniList
    {
        public string isni { get; set; }
    }

}