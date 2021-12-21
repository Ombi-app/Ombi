#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: MovieResponse.cs
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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Ombi.TheMovieDbApi.Models
{

    public class MovieResponse
    {
        public bool adult { get; set; }
        public string backdrop_path { get; set; }
        public BelongsToCollection belongs_to_collection { get; set; }
        public int budget { get; set; }
        public Genre[] genres { get; set; }
        public string homepage { get; set; }
        public int id { get; set; }
        public string imdb_id { get; set; }
        public string original_language { get; set; }
        public string original_title { get; set; }
        public string overview { get; set; }
        public float popularity { get; set; }
        public string poster_path { get; set; }
        public ProductionCompanies[] production_companies { get; set; }
        public ProductionCountries[] production_countries { get; set; }
        public string release_date { get; set; }
        public float revenue { get; set; }
        public float runtime { get; set; }
        public SpokenLanguages[] spoken_languages { get; set; }
        public string status { get; set; }
        public string tagline { get; set; }
        public string title { get; set; }
        public bool video { get; set; }
        public float vote_average { get; set; }
        public int vote_count { get; set; }
        public ReleaseDates release_dates { get; set; }
    }

    public class ReleaseDates
    {
        public List<ReleaseResults> results { get; set; }
    }

    public class ReleaseResults
    {
        public string iso_3166_1 { get; set; }
        public List<ReleaseDate> release_dates { get; set; }
    }

    public class ReleaseDate
    {
        public string iso_639_1 { get; set; }
        public string release_date { get; set; }
        [JsonIgnore]
        public DateTime ReleaseDateTime
        {
            get
            {
                if (DateTime.TryParse(release_date,out var formattedDate))
                {
                    return formattedDate;
                }

                if (DateTime.TryParseExact(release_date, "yyyy-MM-dd hh:mm:ss UTC", 
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var excatDate))
                {
                    return excatDate;
                }
                return DateTime.MinValue;
            }
        }
        public int Type { get; set; }
    }
}