#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CouchPotatoAdd.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System.Collections.Generic;

namespace Ombi.Api.Models.Movie
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
        public List<string> actors { get; set; }
        public List<string> backdrop { get; set; }
        public List<string> backdrop_original { get; set; }
        public List<object> banner { get; set; }
        public List<object> clear_art { get; set; }
        public List<object> disc_art { get; set; }
        public List<object> extra_fanart { get; set; }
        public List<object> extra_thumbs { get; set; }
        public List<object> landscape { get; set; }
        public List<object> logo { get; set; }
        public List<string> poster { get; set; }
        public List<string> poster_original { get; set; }
    }

    public class Info
    {
        public List<string> actor_roles { get; set; }
        public List<string> actors { get; set; }
        public List<string> directors { get; set; }
        public List<string> genres { get; set; }
        public Images images { get; set; }
        public string imdb { get; set; }
        public string mpaa { get; set; }
        public string original_title { get; set; }
        public string plot { get; set; }
        public Rating rating { get; set; }
        public Release_Date release_date { get; set; }
        public string released { get; set; }
        public int runtime { get; set; }
        public string tagline { get; set; }
        public List<string> titles { get; set; }
        public int tmdb_id { get; set; }
        public string type { get; set; }
        public bool via_imdb { get; set; }
        public bool via_tmdb { get; set; }
        public List<string> writers { get; set; }
        public int year { get; set; }
    }

    public class Release_Date
    {
        public bool bluray { get; set; }
        public int dvd { get; set; }
        public int expires { get; set; }
        public int theater { get; set; }
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
        public string _id { get; set; }
        public string _rev { get; set; }
        public string _t { get; set; }
        public object category_id { get; set; }
        public Files files { get; set; }
        public Identifiers identifiers { get; set; }
        public Info info { get; set; }
        public int last_edit { get; set; }
        public string profile_id { get; set; }
        public List<object> releases { get; set; }
        public string status { get; set; }
        public List<object> tags { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }
}