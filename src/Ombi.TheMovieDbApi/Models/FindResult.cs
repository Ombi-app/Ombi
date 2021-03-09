namespace Ombi.Api.TheMovieDb.Models
{
    public class FindResult
    {
        public Movie_Results[] movie_results { get; set; }
        public object[] person_results { get; set; }
        public TvResults[] tv_results { get; set; }
        public object[] tv_episode_results { get; set; }
        public FindSeasonResults[] tv_season_results { get; set; }
    }

    public class FindSeasonResults
    {
        public int show_id { get; set; }
    }

    public class Movie_Results
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
    }


    public class TvResults
    {
        public string original_name { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int vote_count { get; set; }
        public float vote_average { get; set; }
        public string first_air_date { get; set; }
        public string poster_path { get; set; }
        public int[] genre_ids { get; set; }
        public string original_language { get; set; }
        public string backdrop_path { get; set; }
        public string overview { get; set; }
        public string[] origin_country { get; set; }
    }


    public enum ExternalSource
    {
        imdb_id,
        tvdb_id
    }
}