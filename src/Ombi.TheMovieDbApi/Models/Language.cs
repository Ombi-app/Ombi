using Newtonsoft.Json;

namespace Ombi.TheMovieDbApi.Models
{
    public class Language
    {
        [JsonProperty("iso_639_1")]
        public string Id { get; set; }
        [JsonProperty("english_name")]
        public string EnglishName { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}