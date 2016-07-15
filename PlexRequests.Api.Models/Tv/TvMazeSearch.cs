#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: TvMazeSearch.cs
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

namespace PlexRequests.Api.Models.Tv
{
    public class Schedule
    {
        public List<object> days { get; set; }
        public string time { get; set; }
    }

    public class Rating
    {
        public double? average { get; set; }
    }

    public class Country
    {
        public string code { get; set; }
        public string name { get; set; }
        public string timezone { get; set; }
    }

    public class Network
    {
        public Country country { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Externals
    {
        public string imdb { get; set; }
        public int? thetvdb { get; set; }
        public int? tvrage { get; set; }
    }

    public class Image
    {
        public string medium { get; set; }
        public string original { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class Previousepisode
    {
        public string href { get; set; }
    }

    public class Nextepisode
    {
        public string href { get; set; }
    }

    public class Links
    {
        public Nextepisode nextepisode { get; set; }
        public Previousepisode previousepisode { get; set; }
        public Self self { get; set; }
    }

    public class Show
    {
        public Links _links { get; set; }
        public Externals externals { get; set; }
        public List<object> genres { get; set; }
        public int id { get; set; }
        public Image image { get; set; }
        public string language { get; set; }
        public string name { get; set; }
        public Network network { get; set; }
        public string premiered { get; set; }
        public Rating rating { get; set; }
        public int? runtime { get; set; }
        public Schedule schedule { get; set; }
        public string status { get; set; }
        public string summary { get; set; }
        public string type { get; set; }
        public int updated { get; set; }
        public string url { get; set; }
        public object webChannel { get; set; }
        public int weight { get; set; }
    }

    public class TvMazeSearch
    {
        public double score { get; set; }
        public Show show { get; set; }
    }
}