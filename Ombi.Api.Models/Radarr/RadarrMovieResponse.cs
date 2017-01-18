#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: RadarrMovieResponse.cs
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

namespace Ombi.Api.Models.Radarr
{


    public class Image
    {
        public string coverType { get; set; }
        public string url { get; set; }
    }

    public class Ratings
    {
        public int votes { get; set; }
        public double value { get; set; }
    }

    public class RadarrMovieResponse
    {
        public string title { get; set; }
        public string sortTitle { get; set; }
        public int sizeOnDisk { get; set; }
        public string status { get; set; }
        public string overview { get; set; }
        public string inCinemas { get; set; }
        public string physicalRelease { get; set; }
        public List<Image> images { get; set; }
        public string website { get; set; }
        public bool downloaded { get; set; }
        public int year { get; set; }
        public bool hasFile { get; set; }
        public string youTubeTrailerId { get; set; }
        public string studio { get; set; }
        public string path { get; set; }
        public int profileId { get; set; }
        public bool monitored { get; set; }
        public int runtime { get; set; }
        public string lastInfoSync { get; set; }
        public string cleanTitle { get; set; }
        public string imdbId { get; set; }
        public int tmdbId { get; set; }
        public string titleSlug { get; set; }
        public List<string> genres { get; set; }
        public List<object> tags { get; set; }
        public string added { get; set; }
        public Ratings ratings { get; set; }
        public List<string> alternativeTitles { get; set; }
        public int qualityProfileId { get; set; }
        public int id { get; set; }
    }

}