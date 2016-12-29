#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: WatcherListStatusResult.cs
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
using RestSharp.Deserializers;

namespace Ombi.Api.Models.Watcher
{
    public class Quality2
    {
        [JsonProperty("720P")]
        public List<string> Q720P { get; set; }
        [JsonProperty("1080P")]
        public List<string> Q1080P { get; set; }
        [JsonProperty("4K")]
        public List<string> Q4K { get; set; }
        public List<string> SD { get; set; }
    }

    public class Filters
    {
        public string preferredwords { get; set; }
        public string ignoredwords { get; set; }
        public string requiredwords { get; set; }
    }

    public class Quality
    {
        [JsonProperty("Quality")]
        public Quality2 quality { get; set; }
        public Filters Filters { get; set; }
    }

    public class WatcherListStatusResult
    {
        public string status { get; set; }
        public string plot { get; set; }
        public string rated { get; set; }
        public string title { get; set; }
        public object finishedscore { get; set; }
        public string predb { get; set; }
        public string year { get; set; }
        public string poster { get; set; }
        public string tomatourl { get; set; }
        public string released { get; set; }
        public object finisheddate { get; set; }
        public string dvd { get; set; }
        public string tomatorating { get; set; }
        public string imdbid { get; set; }
        public Quality quality { get; set; }

    }

    public class WatcherListStatusResultContainer
    {
        public List<WatcherListStatusResult> Results { get; set; }
        [JsonIgnore]
        public string ErrorMessage { get; set; }
        [JsonIgnore]
        public bool Error { get; set; }
    }
}