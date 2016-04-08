using System.Collections.Generic;

namespace PlexRequests.Api.Models.Movie
{
    public class CouchPotatoMovies
    {
        public List<Movie> movies { get; set; }
        public int total { get; set; }
        public bool success { get; set; }
        public bool empty { get; set; }
    }
}
