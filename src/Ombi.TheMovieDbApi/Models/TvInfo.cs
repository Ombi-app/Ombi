using Newtonsoft.Json;

namespace Ombi.Api.TheMovieDb.Models
{
    public class TvInfo
    {
        public string backdrop_path { get; set; }
        public Created_By[] created_by { get; set; }
        public int[] episode_run_time { get; set; }
        public string first_air_date { get; set; }
        public Genre[] genres { get; set; }
        public string homepage { get; set; }
        public int id { get; set; }
        public bool in_production { get; set; }
        public string[] languages { get; set; }
        public string last_air_date { get; set; }
        public string name { get; set; }
        public Network[] networks { get; set; }
        public int number_of_episodes { get; set; }
        public int number_of_seasons { get; set; }
        public string[] origin_country { get; set; }
        public string original_language { get; set; }
        public string original_name { get; set; }
        public string overview { get; set; }
        public float popularity { get; set; }
        public string poster_path { get; set; }
        public Production_Companies[] production_companies { get; set; }
        public Season[] seasons { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public float vote_average { get; set; }
        public int vote_count { get; set; }
        [JsonProperty("external_ids")] public TvExternalIds TvExternalIds { get; set; }
    }

    public class Created_By
    {
        public int id { get; set; }
        public string name { get; set; }
        public int gender { get; set; }
        public string profile_path { get; set; }
    }

    public class Genre
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Network
    {
        public string name { get; set; }
        public int id { get; set; }
        public string logo_path { get; set; }
        public string origin_country { get; set; }
    }

    public class Production_Companies
    {
        public int id { get; set; }
        public string logo_path { get; set; }
        public string name { get; set; }
        public string origin_country { get; set; }
    }

    public class Season
    {
        public string air_date { get; set; }
        public int episode_count { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string overview { get; set; }
        public string poster_path { get; set; }
        public int season_number { get; set; }
    }

    public class TvExternalIds
    {
        [JsonProperty("imdb_id")] public string ImdbId { get; set; }
        [JsonProperty("tvdb_id")] public string TvDbId { get; set; }
        [JsonProperty("tvrage_id")] public string TvRageId { get; set; }
        [JsonProperty("facebook_id")] public string FacebookId { get; set; }
        [JsonProperty("instagram_id")] public string InstagramId { get; set; }
        [JsonProperty("twitter_id")] public string TwitterHandle { get; set; }
    }
}