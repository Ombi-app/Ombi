using System.Collections.Generic;
using Newtonsoft.Json;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.Api.TheMovieDb.Models
{
    public class FullMovieInfo
    {
        [JsonProperty("adult")]
        public bool Adult { get; set; }
        [JsonProperty("backdrop_path")]
        public string BackdropPath { get; set; }
        [JsonProperty("belongs_to_collection")]
        public BelongsToCollection BelongsToCollection { get; set; }
        [JsonProperty("budget")]
        public int Budget { get; set; }
        [JsonProperty("genres")]
        public Genre[] Genres { get; set; }
        [JsonProperty("homepage")]
        public string Homepage { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("imdb_id")]
        public string ImdbId { get; set; }
        [JsonProperty("original_language")]
        public string OriginalLanguage { get; set; }
        [JsonProperty("original_title")]
        public string OriginalTitle { get; set; }
        [JsonProperty("overview")]
        public string Overview { get; set; }
        [JsonProperty("popularity")]
        public float Popularity { get; set; }
        [JsonProperty("poster_path")]
        public string PosterPath { get; set; }
        [JsonProperty("production_companies")]
        public Production_Companies[] ProductionCompanies { get; set; }
        [JsonProperty("production_countries")]
        public Production_Countries[] ProductionCountries { get; set; }
        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }
        [JsonProperty("revenue")]
        public float Revenue { get; set; }
        [JsonProperty("runtime")]
        public long Runtime { get; set; }
        [JsonProperty("spoken_languages")]
        public Spoken_Languages[] SpokenLanguages { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("tagline")]
        public string Tagline { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("video")]
        public bool Video { get; set; }
        [JsonProperty("vote_average")]
        public float VoteAverage { get; set; }
        [JsonProperty("vote_count")]
        public int VoteCount { get; set; }
        [JsonProperty("videos")]
        public Videos Videos { get; set; }
        [JsonProperty("credits")]
        public Credits Credits { get; set; }
        [JsonProperty("similar")]
        public Similar Similar { get; set; }
        [JsonProperty("recommendations")]
        public Recommendations Recommendations { get; set; }
        [JsonProperty("release_dates")]
        public ReleaseDates ReleaseDates { get; set; }
        [JsonProperty("external_ids")]
        public ExternalIds ExternalIds { get; set; }
        [JsonProperty("keywords")]
        public Keywords Keywords { get; set; }
        
        //[JsonProperty("images")]
        //public List<Images> Images { get; set; } // add images to append_to_response
    }

    public class Images
    {
        [JsonProperty("backdrops")]
        public List<ImageContent> Backdrops { get; set; }
        [JsonProperty("posters")]
        public List<ImageContent> Posters { get; set; }
    }

    public class ImageContent
    {
        [JsonProperty("file_path")]
        public string FilePath { get; set; }
    }

    public class Keywords
    {
        [JsonProperty("keywords")]
        public List<KeywordsValue> KeywordsValue { get; set; }
    }

    public class KeywordsValue
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Videos
    {
        public Result[] results { get; set; }
    }

    public class Result
    {
        public string id { get; set; }
        public string iso_639_1 { get; set; }
        public string iso_3166_1 { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string site { get; set; }
        public int size { get; set; }
        public string type { get; set; }
    }

    public class ExternalIds
    {
        [JsonProperty("imdb_id")]
        public string ImdbId { get; set; }
        [JsonProperty("facebook_id")]
        public string FacebookId { get; set; }
        [JsonProperty("instagram_id")]
        public string InstagramId { get; set; }
        [JsonProperty("twitter_id")]
        public string TwitterId { get; set; }
    }

    public class Credits
    {
        public FullMovieCast[] cast { get; set; }
        public FullMovieCrew[] crew { get; set; }
    }

    public class FullMovieCast
    {
        public int cast_id { get; set; }
        public string character { get; set; }
        public string credit_id { get; set; }
        public int gender { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int order { get; set; }
        public string profile_path { get; set; }
    }

    public class FullMovieCrew
    {
        public string credit_id { get; set; }
        public string department { get; set; }
        public int gender { get; set; }
        public int id { get; set; }
        public string job { get; set; }
        public string name { get; set; }
        public string profile_path { get; set; }
    }

    public class Similar
    {
        public int page { get; set; }
        public SimilarResults[] results { get; set; }
        public int total_pages { get; set; }
        public int total_results { get; set; }
    }

    public class SimilarResults
    {
        public bool adult { get; set; }
        public string backdrop_path { get; set; }
        public int[] genre_ids { get; set; }
        public int id { get; set; }
        public string original_language { get; set; }
        public string original_title { get; set; }
        public string overview { get; set; }
        public string poster_path { get; set; }
        public string release_date { get; set; }
        public string title { get; set; }
        public bool video { get; set; }
        public float vote_average { get; set; }
        public int vote_count { get; set; }
        public float popularity { get; set; }
    }

    public class Recommendations
    {
        public int page { get; set; }
        public RecommendationResults[] results { get; set; }
        public int total_pages { get; set; }
        public int total_results { get; set; }
    }

    public class RecommendationResults
    {
        public bool adult { get; set; }
        public string backdrop_path { get; set; }
        public int[] genre_ids { get; set; }
        public int id { get; set; }
        public string original_language { get; set; }
        public string original_title { get; set; }
        public string overview { get; set; }
        public string poster_path { get; set; }
        public string release_date { get; set; }
        public string title { get; set; }
        public bool video { get; set; }
        public float vote_average { get; set; }
        public int vote_count { get; set; }
        public float popularity { get; set; }
    }

    public class Production_Countries
    {
        public string iso_3166_1 { get; set; }
        public string name { get; set; }
    }

    public class Spoken_Languages
    {
        public string iso_639_1 { get; set; }
        public string name { get; set; }
    }

}