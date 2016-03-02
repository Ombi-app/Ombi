#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: TvSearchResult.cs
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

namespace RequestPlex.Api.Models.Tv
{
    public class TvShowSearchResult
    {
        public int id { get; set; }
        public int airedSeason { get; set; }
        public int airedEpisodeNumber { get; set; }
        public string episodeName { get; set; }
        public string firstAired { get; set; }
        public string guestStars { get; set; }
        public string director { get; set; }
        public List<string> writers { get; set; }
        public string overview { get; set; }
        public string productionCode { get; set; }
        public string showUrl { get; set; }
        public int lastUpdated { get; set; }
        public string dvdDiscid { get; set; }
        public int dvdSeason { get; set; }
        public int dvdEpisodeNumber { get; set; }
        public int dvdChapter { get; set; }
        public int absoluteNumber { get; set; }
        public string filename { get; set; }
        public string seriesId { get; set; }
        public string lastUpdatedBy { get; set; }
        public int airsAfterSeason { get; set; }
        public int airsBeforeSeason { get; set; }
        public int airsBeforeEpisode { get; set; }
        public string thumbAuthor { get; set; }
        public string thumbAdded { get; set; }
        public string thumbWidth { get; set; }
        public string thumbHeight { get; set; }
        public string imdbId { get; set; }
        public int siteRating { get; set; }
    }

    public class TvSearchResult
    {
        public List<TvShowSearchResult> data { get; set; }
    }
}
