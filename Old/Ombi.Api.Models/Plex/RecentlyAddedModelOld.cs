#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RecentlyAddedModelOld.cs
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

namespace Ombi.Api.Models.Plex
{
    public class RecentlyAddedChild
    {
        public string _elementType { get; set; }
        public int ratingKey { get; set; }
        public string key { get; set; }
        public int parentRatingKey { get; set; }
        public int grandparentRatingKey { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string grandparentKey { get; set; }
        public string parentKey { get; set; }
        public string grandparentTitle { get; set; }
        public string summary { get; set; }
        public int index { get; set; }
        public int parentIndex { get; set; }
        public string thumb { get; set; }
        public string art { get; set; }
        public string grandparentThumb { get; set; }
        public string grandparentArt { get; set; }
        public int duration { get; set; }
        public int addedAt { get; set; }
        public int updatedAt { get; set; }
        public string chapterSource { get; set; }
        public List<Child2> _children { get; set; }
        public string contentRating { get; set; }
        public int? year { get; set; }
        public string parentThumb { get; set; }
        public string grandparentTheme { get; set; }
        public string originallyAvailableAt { get; set; }
        public string titleSort { get; set; }
        public int? viewCount { get; set; }
        public int? lastViewedAt { get; set; }
        public int? viewOffset { get; set; }
        public string rating { get; set; }
        public string studio { get; set; }
        public string tagline { get; set; }
        public string originalTitle { get; set; }
        public string audienceRating { get; set; }
        public string audienceRatingImage { get; set; }
        public string ratingImage { get; set; }
    }
    public class Child3
    {
        public string _elementType { get; set; }
        public string id { get; set; }
        public string key { get; set; }
        public double duration { get; set; }
        public string file { get; set; }
        public double size { get; set; }
        public string audioProfile { get; set; }
        public string container { get; set; }
        public string videoProfile { get; set; }
        public string deepAnalysisVersion { get; set; }
        public string requiredBandwidths { get; set; }
        public string hasThumbnail { get; set; }
        public bool? has64bitOffsets { get; set; }
        public bool? optimizedForStreaming { get; set; }
        public bool? hasChapterTextStream { get; set; }
    }

    public class Child2
    {
        public string _elementType { get; set; }
        public string videoResolution { get; set; }
        public int id { get; set; }
        public int duration { get; set; }
        public int bitrate { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string aspectRatio { get; set; }
        public int audioChannels { get; set; }
        public string audioCodec { get; set; }
        public string videoCodec { get; set; }
        public string container { get; set; }
        public string videoFrameRate { get; set; }
        public string audioProfile { get; set; }
        public string videoProfile { get; set; }
        public List<Child3> _children { get; set; }
        public string tag { get; set; }
    }

    public class RecentlyAddedModelOld
    {
        public string _elementType { get; set; }
        public string allowSync { get; set; }
        public string art { get; set; }
        public string identifier { get; set; }
        public string librarySectionID { get; set; }
        public string librarySectionTitle { get; set; }
        public string librarySectionUUID { get; set; }
        public string mediaTagPrefix { get; set; }
        public string mediaTagVersion { get; set; }
        public string mixedParents { get; set; }
        public string nocache { get; set; }
        public string thumb { get; set; }
        public string title1 { get; set; }
        public string title2 { get; set; }
        public string viewGroup { get; set; }
        public string viewMode { get; set; }
        public List<RecentlyAddedChild> _children { get; set; }
    }


    // 1.3 and forward!
    public class PartRecentlyAdded
    {
        public int id { get; set; }
        public string key { get; set; }
        public int duration { get; set; }
        public string file { get; set; }
        public double size { get; set; }
        public string audioProfile { get; set; }
        public string container { get; set; }
        public string videoProfile { get; set; }
        public string deepAnalysisVersion { get; set; }
        public string requiredBandwidths { get; set; }
    }

    public class Medium
    {
        public string videoResolution { get; set; }
        public int id { get; set; }
        public int duration { get; set; }
        public int bitrate { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public double aspectRatio { get; set; }
        public int audioChannels { get; set; }
        public string audioCodec { get; set; }
        public string videoCodec { get; set; }
        public string container { get; set; }
        public string videoFrameRate { get; set; }
        public string audioProfile { get; set; }
        public string videoProfile { get; set; }
        public List<PartRecentlyAdded> Part { get; set; }
    }

    public class DirectorRecentlyAdded
    {
        public string tag { get; set; }
    }

    public class WriterRecentlyAdded
    {
        public string tag { get; set; }
    }

    public class Metadata
    {
        public string ratingKey { get; set; }
        public string key { get; set; }
        public string parentRatingKey { get; set; }
        public string grandparentRatingKey { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string titleSort { get; set; }
        public string grandparentKey { get; set; }
        public string parentKey { get; set; }
        public string grandparentTitle { get; set; }
        public string contentRating { get; set; }
        public string summary { get; set; }
        public int index { get; set; }
        public int parentIndex { get; set; }
        public int year { get; set; }
        public string thumb { get; set; }
        public string art { get; set; }
        public string parentThumb { get; set; }
        public string grandparentThumb { get; set; }
        public string grandparentArt { get; set; }
        public string grandparentTheme { get; set; }
        public int duration { get; set; }
        public string originallyAvailableAt { get; set; }
        public int addedAt { get; set; }
        public int updatedAt { get; set; }
        public List<Medium> Media { get; set; }
        public List<DirectorRecentlyAdded> Director { get; set; }
        public List<WriterRecentlyAdded> Writer { get; set; }
        public int? viewCount { get; set; }
        public int? lastViewedAt { get; set; }
        public double? rating { get; set; }
    }

    public class MediaContainer
    {
        public double size { get; set; }
        public double totalSize { get; set; }
        public bool allowSync { get; set; }
        public string art { get; set; }
        public string identifier { get; set; }
        public int librarySectionID { get; set; }
        public string librarySectionTitle { get; set; }
        public string librarySectionUUID { get; set; }
        public string mediaTagPrefix { get; set; }
        public int mediaTagVersion { get; set; }
        public bool mixedParents { get; set; }
        public bool nocache { get; set; }
        public int offset { get; set; }
        public string thumb { get; set; }
        public string title1 { get; set; }
        public string title2 { get; set; }
        public string viewGroup { get; set; }
        public int viewMode { get; set; }
        public List<Metadata> Metadata { get; set; }
    }

    public class PlexRecentlyAddedModel
    {
        public MediaContainer MediaContainer { get; set; }
    }
}