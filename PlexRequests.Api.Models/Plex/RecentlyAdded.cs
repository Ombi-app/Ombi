#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RecentlyAdded.cs
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

namespace PlexRequests.Api.Models.Plex
{
    public class RecentlyAddedChild
    {
        public string _elementType { get; set; }
        public string allowSync { get; set; }
        public string librarySectionID { get; set; }
        public string librarySectionTitle { get; set; }
        public string librarySectionUUID { get; set; }
        public int ratingKey { get; set; }
        public string key { get; set; }
        public int parentRatingKey { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string parentKey { get; set; }
        public string parentTitle { get; set; }
        public string parentSummary { get; set; }
        public string summary { get; set; }
        public int index { get; set; }
        public int parentIndex { get; set; }
        public string thumb { get; set; }
        public string art { get; set; }
        public string parentThumb { get; set; }
        public int leafCount { get; set; }
        public int viewedLeafCount { get; set; }
        public int addedAt { get; set; }
        public int updatedAt { get; set; }
        public List<object> _children { get; set; }
        public string studio { get; set; }
        public string contentRating { get; set; }
        public string rating { get; set; }
        public int? viewCount { get; set; }
        public int? lastViewedAt { get; set; }
        public int? year { get; set; }
        public int? duration { get; set; }
        public string originallyAvailableAt { get; set; }
        public string chapterSource { get; set; }
        public string parentTheme { get; set; }
        public string titleSort { get; set; }
        public string tagline { get; set; }
        public int? viewOffset { get; set; }
        public string originalTitle { get; set; }
    }

    public class RecentlyAdded
    {
        public string _elementType { get; set; }
        public string allowSync { get; set; }
        public string identifier { get; set; }
        public string mediaTagPrefix { get; set; }
        public string mediaTagVersion { get; set; }
        public string mixedParents { get; set; }
        public List<RecentlyAddedChild> _children { get; set; }
    }
}