namespace Ombi.Api.TheMovieDb.Models
{
    public class MovieResponseDto
    {
        public bool Adult { get; set; }
        public string BackdropPath { get; set; }
        public int Budget { get; set; }
        public GenreDto[] Genres { get; set; }
        public string Homepage { get; set; }
        public int Id { get; set; }
        public string ImdbId { get; set; }
        public string OriginalLanguage { get; set; }
        public string OriginalTitle { get; set; }
        public string Overview { get; set; }
        public float Popularity { get; set; }
        public string PosterPath { get; set; }
        public string ReleaseDate { get; set; }
        public float Revenue { get; set; }
        public float Runtime { get; set; }
        public string Status { get; set; }
        public string Tagline { get; set; }
        public string Title { get; set; }
        public bool Video { get; set; }
        public float VoteAverage { get; set; }
        public int VoteCount { get; set; }
    }
}