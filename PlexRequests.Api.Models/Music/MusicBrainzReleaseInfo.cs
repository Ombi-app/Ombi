#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: MusicBrainzReleaseInfo.cs
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

namespace PlexRequests.Api.Models.Music
{
    public class CoverArtArchive
    {
        public int count { get; set; }
        public bool back { get; set; }
        public bool artwork { get; set; }
        public bool front { get; set; }
        public bool darkened { get; set; }
    }


    public class MusicBrainzReleaseInfo
    {
        public string date { get; set; }
        public string status { get; set; }
        public string asin { get; set; }
        public string title { get; set; }
        public string quality { get; set; }
        public string country { get; set; }
        public string packaging { get; set; }

        [JsonProperty(PropertyName = "text-representation")]
        public TextRepresentation TextRepresentation { get; set; }

        [JsonProperty(PropertyName = "cover-art-archive")]
        public CoverArtArchive CoverArtArchive { get; set; }
        public string barcode { get; set; }
        public string disambiguation { get; set; }

        [JsonProperty(PropertyName = "release-events")]
        public List<ReleaseEvent> ReleaseEvents { get; set; }
        public string id { get; set; }
    }

}