namespace Ombi.Api.TheMovieDb.Models
{
    public class MovieDbImages
    {
        public Backdrop[] backdrops { get; set; }
        public int id { get; set; }
        public Logo[] logos { get; set; }
        public Poster[] posters { get; set; }
    }

    public class Backdrop
    {
        public float aspect_ratio { get; set; }
        public int height { get; set; }
        public string iso_639_1 { get; set; }
        public string file_path { get; set; }
        public float vote_average { get; set; }
        public int vote_count { get; set; }
        public int width { get; set; }
    }

    public class Logo
    {
        public float aspect_ratio { get; set; }
        public int height { get; set; }
        public string iso_639_1 { get; set; }
        public string file_path { get; set; }
        public float vote_average { get; set; }
        public int vote_count { get; set; }
        public int width { get; set; }
    }

    public class Poster
    {
        public float aspect_ratio { get; set; }
        public int height { get; set; }
        public string iso_639_1 { get; set; }
        public string file_path { get; set; }
        public float vote_average { get; set; }
        public int vote_count { get; set; }
        public int width { get; set; }
    }

}
