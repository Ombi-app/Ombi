using System.Collections.Generic;

namespace Ombi.Api.CouchPotato.Models
{
    public class CouchPotatoMovies
    {
        public List<Movie> movies { get; set; }
        public int total { get; set; }
        public bool success { get; set; }
        public bool empty { get; set; }
    }

    public class Movie
    {
        public string _id { get; set; }
        public string _rev { get; set; }
        public string _t { get; set; }
        public object category_id { get; set; }
        public Info info { get; set; }
        public int last_edit { get; set; }
        public string profile_id { get; set; }
        public List<object> releases { get; set; }
        public string status { get; set; }
        public List<object> tags { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }
    public class CouchPotatoAdd
    {
        public Movie movie { get; set; }
        public bool success { get; set; }
    }


    public class Info
    {   public string imdb { get; set; }
        public int tmdb_id { get; set; }
    }


}