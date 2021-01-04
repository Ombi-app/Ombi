using System.Collections.Generic;

namespace Ombi.Api.RottenTomatoes.Models
{
    public class RottenTomatoesMovieResponse
    {
        public int total { get; set; }
        public List<Movie> movies { get; set; }
    }

    public class Movie
    {
        public string id { get; set; }
        public string title { get; set; }
        public int year { get; set; }
        public string mpaa_rating { get; set; }
        public object runtime { get; set; }
        public string critics_consensus { get; set; }
        public MovieRatings ratings { get; set; }
        public Links links { get; set; }
    }

    public class MovieRatings
    {
        public string critics_rating { get; set; }
        public int critics_score { get; set; }
        public string audience_rating { get; set; }
        public int audience_score { get; set; }
    }

    public class Links
    {
        public string alternate { get; set; }
    }
}
