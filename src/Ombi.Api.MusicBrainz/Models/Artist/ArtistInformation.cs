using Newtonsoft.Json;

namespace Ombi.Api.MusicBrainz.Models.Artist
{

    public class ArtistInformation
    {
        public Begin_Area begin_area { get; set; }
        [JsonProperty("gender-id")]
        public object genderid { get; set; }
        public string type { get; set; }
        [JsonProperty("life-span")]
        public LifeSpan lifespan { get; set; }
        public string name { get; set; }
        
        [JsonProperty("type-id")]
        public string typeid { get; set; }
        public object end_area { get; set; }
        public object[] ipis { get; set; }
        [JsonProperty("sort-name")]
        public string sortname { get; set; }
        public string[] isnis { get; set; }
        public object gender { get; set; }
        public string id { get; set; }
        public Area area { get; set; }
        public string disambiguation { get; set; }
        public string country { get; set; }
    }

    public class Begin_Area
    {
        public string disambiguation { get; set; }
        [JsonProperty("sort-name")]
        public string sortname { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public class LifeSpan
    {
        public bool ended { get; set; }
        public string end { get; set; }
        public string begin { get; set; }
    }

    public class Area
    {
        public string name { get; set; }
        [JsonProperty("sort-name")]
        public string sortname { get; set; }
        public string disambiguation { get; set; }
        public string id { get; set; }
        
        [JsonProperty("iso-3166-1-codes")]
        public string[] iso31661codes { get; set; }
    }

}