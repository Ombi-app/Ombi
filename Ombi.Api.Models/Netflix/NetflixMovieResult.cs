#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: NetflixMovieResult.cs
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

namespace Ombi.Api.Models.Netflix
{
    public class NetflixMovieResult
    {
        [JsonProperty(PropertyName= "unit")]
        public int Unit { get; set; }

        [JsonProperty(PropertyName = "show_id")]
        public int ShowId { get; set; }

        [JsonProperty(PropertyName = "show_title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "release_year")]
        public string ReleaseYear { get; set; }
        [JsonProperty(PropertyName = "rating")]
        public string Rating { get; set; }
        [JsonProperty(PropertyName = "Category")]
        public string Category { get; set; }
        [JsonProperty(PropertyName = "show_cast")]
        public string ShowCast { get; set; }
        [JsonProperty(PropertyName = "director")]
        public string Director { get; set; }
        [JsonProperty(PropertyName = "summary")]
        public string Summary { get; set; }
        [JsonProperty(PropertyName = "poster")]
        public string Poster { get; set; }
        [JsonProperty(PropertyName = "mediatype")]
        public string Mediatype { get; set; }
        [JsonProperty(PropertyName = "runtime")]
        public string Runtime { get; set; }
    }
}