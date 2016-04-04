#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: MusicBrainzSearchResults.cs
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
    public class TextRepresentation
    {
        public string language { get; set; }
        public string script { get; set; }
    }

    public class Alias
    {
        [JsonProperty(PropertyName = "sort-name")]
        public string SortName { get; set; }
        public string name { get; set; }
        public object locale { get; set; }
        public string type { get; set; }
        public object primary { get; set; }
        [JsonProperty(PropertyName = "begin-date")]
        public object BeginDate { get; set; }
        [JsonProperty(PropertyName = "end-date")]
        public object EndDate { get; set; }
    }

    public class Artist
    {
        public string id { get; set; }
        public string name { get; set; }
        [JsonProperty(PropertyName = "sort-date")]
        public string SortName { get; set; }
        public string disambiguation { get; set; }
        public List<Alias> aliases { get; set; }
    }

    public class ArtistCredit
    {
        public Artist artist { get; set; }
        public string name { get; set; }
        public string joinphrase { get; set; }
    }

    public class ReleaseGroup
    {
        public string id { get; set; }
        [JsonProperty(PropertyName = "primary-type")]
        public string PrimaryType { get; set; }
        [JsonProperty(PropertyName = "secondary-types")]
        public List<string> SecondaryTypes { get; set; }
    }

    public class Area
    {
        public string id { get; set; }
        public string name { get; set; }
        [JsonProperty(PropertyName = "sort-name")]
        public string SortName { get; set; }
        [JsonProperty(PropertyName = "iso-3166-1-codes")]
        public List<string> ISO31661Codes { get; set; }
    }

    public class ReleaseEvent
    {
        public string date { get; set; }
        public Area area { get; set; }
    }

    public class Label
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class LabelInfo
    {
        [JsonProperty(PropertyName = "catalog-number")]
        public string CatalogNumber { get; set; }
        public Label label { get; set; }
    }

    public class Medium
    {
        public string format { get; set; }
        [JsonProperty(PropertyName = "disc-count")]
        public int DiscCount { get; set; }
        [JsonProperty(PropertyName = "catalog-number")]
        public int CatalogNumber { get; set; }
    }

    public class Release
    {
        public string id { get; set; }
        public string score { get; set; }
        public int count { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public string disambiguation { get; set; }
        public string packaging { get; set; }

        [JsonProperty(PropertyName = "text-representation")]
        public TextRepresentation TextRepresentation { get; set; }
        [JsonProperty(PropertyName = "artist-credit")]
        public List<ArtistCredit> ArtistCredit { get; set; }
        [JsonProperty(PropertyName = "release-group")]
        public ReleaseGroup ReleaseGroup { get; set; }
        public string date { get; set; }
        public string country { get; set; }
        [JsonProperty(PropertyName = "release-events")]
        public List<ReleaseEvent> ReleaseEvents { get; set; }
        public string barcode { get; set; }
        public string asin { get; set; }
        [JsonProperty(PropertyName = "label-info")]
        public List<LabelInfo> LabelInfo { get; set; }
        [JsonProperty(PropertyName = "track-count")]
        public int TrackCount { get; set; }
        public List<Medium> media { get; set; }
    }

    public class MusicBrainzSearchResults
    {
        public string created { get; set; }
        public int count { get; set; }
        public int offset { get; set; }
        public List<Release> releases { get; set; }
    }

}