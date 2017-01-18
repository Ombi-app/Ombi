#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: RadarrAddMovie.cs
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
using Newtonsoft.Json;
using Ombi.Api.Models.Sonarr;

namespace Ombi.Api.Models.Radarr
{
    public class RadarrAddMovie
    {

        public RadarrAddMovie()
        {
            images = new List<string>();
        }
        public RadarrError Error { get; set; }
        public RadarrAddOptions addOptions { get; set; }
        public string title { get; set; }
        public string rootFolderPath { get; set; }
        public int qualityProfileId { get; set; }
        public bool monitored { get; set; }
        public int tmdbId { get; set; }
        public List<string> images { get; set; }
        public string cleanTitle { get; set; }
        public string imdbId { get; set; }
        public string titleSlug { get; set; }
        public int id { get; set; }

    }
}