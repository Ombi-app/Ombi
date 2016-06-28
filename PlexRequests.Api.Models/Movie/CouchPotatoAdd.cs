using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlexRequests.Api.Models.Movie
{

    public class CouchPotatoAdd
    {
        public Movie movie { get; set; }
        public bool success { get; set; }
    }
    public class Rating
    {
        public List<string> imdb { get; set; }
    }



    public class Images
    {
        public List<object> disc_art { get; set; }
        public List<string> poster { get; set; }
        public List<string> backdrop { get; set; }
        public List<object> extra_thumbs { get; set; }
        public List<string> poster_original { get; set; }
        public List<string> actors { get; set; }
        public List<string> backdrop_original { get; set; }
        public List<object> clear_art { get; set; }
        public List<object> logo { get; set; }
        public List<object> banner { get; set; }
        public List<object> landscape { get; set; }
        public List<object> extra_fanart { get; set; }
    }

    public class Info
    {
        public Rating rating { get; set; }
        public List<string> genres { get; set; }
        public int tmdb_id { get; set; }
        public string plot { get; set; }
        public string tagline { get; set; }
        public Release_Date release_date { get; set; }
        public int year { get; set; }
        public string original_title { get; set; }
        public List<string> actor_roles { get; set; }
        public bool via_imdb { get; set; }
        public Images images { get; set; }
        public List<string> directors { get; set; }
        public List<string> titles { get; set; }
        public string imdb { get; set; }
        public string mpaa { get; set; }
        public bool via_tmdb { get; set; }
        public List<string> actors { get; set; }
        public List<string> writers { get; set; }
        public int runtime { get; set; }
        public string type { get; set; }
        public string released { get; set; }
    }

    public class Release_Date
    {
        public int dvd { get; set; }
        public int expires { get; set; }
        public int theater { get; set; }
        public bool bluray { get; set; }
    }

    public class Files
    {
        public List<string> image_poster { get; set; }
    }

    public class Identifiers
    {
        public string imdb { get; set; }
    }

    public class Movie
    {
        public string status { get; set; }
        public Info info { get; set; }
        public string _t { get; set; }
        public List<object> releases { get; set; }
        public string title { get; set; }
        public string _rev { get; set; }
        public string profile_id { get; set; }
        public string _id { get; set; }
        public List<object> tags { get; set; }
        public int last_edit { get; set; }
        public object category_id { get; set; }
        public string type { get; set; }
        public Files files { get; set; }
        public Identifiers identifiers { get; set; }
    }


}
