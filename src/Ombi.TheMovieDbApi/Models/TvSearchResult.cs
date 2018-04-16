namespace Ombi.Api.TheMovieDb.Models
{
    public class TvSearchResult
    {
        public string PosterPath { get; set; }
        public string Overview { get; set; }
        public string ReleaseDate { get; set; }
        public int?[] GenreIds { get; set; }
        public int Id { get; set; }
        public string OriginalName { get; set; }
        public string OriginalLanguage { get; set; }
        public string Name { get; set; }
        public string BackdropPath { get; set; }
        public float Popularity { get; set; }
        public int VoteCount { get; set; }
        public float VoteAverage { get; set; }
    }
}