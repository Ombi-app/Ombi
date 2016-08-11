#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexSearch.cs
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
using System.Xml.Serialization;

namespace PlexRequests.Api.Models.Plex
{
    [XmlRoot(ElementName = "Part")]
    public class Part
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlAttribute(AttributeName = "duration")]
        public string Duration { get; set; }
        [XmlAttribute(AttributeName = "file")]
        public string File { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
        [XmlAttribute(AttributeName = "audioProfile")]
        public string AudioProfile { get; set; }
        [XmlAttribute(AttributeName = "container")]
        public string Container { get; set; }
        [XmlAttribute(AttributeName = "videoProfile")]
        public string VideoProfile { get; set; }
        [XmlAttribute(AttributeName = "has64bitOffsets")]
        public string Has64bitOffsets { get; set; }
        [XmlAttribute(AttributeName = "hasChapterTextStream")]
        public string HasChapterTextStream { get; set; }
        [XmlAttribute(AttributeName = "optimizedForStreaming")]
        public string OptimizedForStreaming { get; set; }
    }

    [XmlRoot(ElementName = "Media")]
    public class Media
    {
        [XmlElement(ElementName = "Part")]
        public Part Part { get; set; }
        [XmlAttribute(AttributeName = "videoResolution")]
        public string VideoResolution { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "duration")]
        public string Duration { get; set; }
        [XmlAttribute(AttributeName = "bitrate")]
        public string Bitrate { get; set; }
        [XmlAttribute(AttributeName = "width")]
        public string Width { get; set; }
        [XmlAttribute(AttributeName = "height")]
        public string Height { get; set; }
        [XmlAttribute(AttributeName = "aspectRatio")]
        public string AspectRatio { get; set; }
        [XmlAttribute(AttributeName = "audioChannels")]
        public string AudioChannels { get; set; }
        [XmlAttribute(AttributeName = "audioCodec")]
        public string AudioCodec { get; set; }
        [XmlAttribute(AttributeName = "videoCodec")]
        public string VideoCodec { get; set; }
        [XmlAttribute(AttributeName = "container")]
        public string Container { get; set; }
        [XmlAttribute(AttributeName = "videoFrameRate")]
        public string VideoFrameRate { get; set; }
        [XmlAttribute(AttributeName = "audioProfile")]
        public string AudioProfile { get; set; }
        [XmlAttribute(AttributeName = "videoProfile")]
        public string VideoProfile { get; set; }
        [XmlAttribute(AttributeName = "optimizedForStreaming")]
        public string OptimizedForStreaming { get; set; }
        [XmlAttribute(AttributeName = "has64bitOffsets")]
        public string Has64bitOffsets { get; set; }
    }

    [XmlRoot(ElementName = "Genre")]
    public class Genre
    {
        [XmlAttribute(AttributeName = "tag")]
        public string Tag { get; set; }
    }

    [XmlRoot(ElementName = "Writer")]
    public class Writer
    {
        [XmlAttribute(AttributeName = "tag")]
        public string Tag { get; set; }
    }

    [XmlRoot(ElementName = "Director")]
    public class Director
    {
        [XmlAttribute(AttributeName = "tag")]
        public string Tag { get; set; }
    }

    [XmlRoot(ElementName = "Country")]
    public class Country
    {
        [XmlAttribute(AttributeName = "tag")]
        public string Tag { get; set; }
    }

    [XmlRoot(ElementName = "Role")]
    public class Role
    {
        [XmlAttribute(AttributeName = "tag")]
        public string Tag { get; set; }
    }

    [XmlRoot(ElementName = "Video")]
    public class Video
    {
        public string ProviderId { get; set; }
        [XmlAttribute(AttributeName = "guid")]
        public string Guid { get; set; }
        [XmlElement(ElementName = "Media")]
        public List<Media> Media { get; set; }
        [XmlElement(ElementName = "Genre")]
        public List<Genre> Genre { get; set; }
        [XmlElement(ElementName = "Writer")]
        public List<Writer> Writer { get; set; }
        [XmlElement(ElementName = "Director")]
        public Director Director { get; set; }
        [XmlElement(ElementName = "Country")]
        public Country Country { get; set; }
        [XmlElement(ElementName = "Role")]
        public List<Role> Role { get; set; }
        [XmlAttribute(AttributeName = "allowSync")]
        public string AllowSync { get; set; }
        [XmlAttribute(AttributeName = "librarySectionID")]
        public string LibrarySectionID { get; set; }
        [XmlAttribute(AttributeName = "librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }
        [XmlAttribute(AttributeName = "librarySectionUUID")]
        public string LibrarySectionUUID { get; set; }
        [XmlAttribute(AttributeName = "personal")]
        public string Personal { get; set; }
        [XmlAttribute(AttributeName = "sourceTitle")]
        public string SourceTitle { get; set; }
        [XmlAttribute(AttributeName = "ratingKey")]
        public string RatingKey { get; set; }
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlAttribute(AttributeName = "studio")]
        public string Studio { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
        [XmlAttribute(AttributeName = "contentRating")]
        public string ContentRating { get; set; }
        [XmlAttribute(AttributeName = "summary")]
        public string Summary { get; set; }
        [XmlAttribute(AttributeName = "rating")]
        public string Rating { get; set; }
        [XmlAttribute(AttributeName = "audienceRating")]
        public string AudienceRating { get; set; }
        [XmlAttribute(AttributeName = "year")]
        public string Year { get; set; }
        [XmlAttribute(AttributeName = "tagline")]
        public string Tagline { get; set; }
        [XmlAttribute(AttributeName = "thumb")]
        public string Thumb { get; set; }
        [XmlAttribute(AttributeName = "art")]
        public string Art { get; set; }
        [XmlAttribute(AttributeName = "duration")]
        public string Duration { get; set; }
        [XmlAttribute(AttributeName = "originallyAvailableAt")]
        public string OriginallyAvailableAt { get; set; }
        [XmlAttribute(AttributeName = "addedAt")]
        public string AddedAt { get; set; }
        [XmlAttribute(AttributeName = "updatedAt")]
        public string UpdatedAt { get; set; }
        [XmlAttribute(AttributeName = "audienceRatingImage")]
        public string AudienceRatingImage { get; set; }
        [XmlAttribute(AttributeName = "chapterSource")]
        public string ChapterSource { get; set; }
        [XmlAttribute(AttributeName = "ratingImage")]
        public string RatingImage { get; set; }
        [XmlAttribute(AttributeName = "titleSort")]
        public string TitleSort { get; set; }
        [XmlAttribute(AttributeName = "parentRatingKey")]
        public string ParentRatingKey { get; set; }
        [XmlAttribute(AttributeName = "grandparentRatingKey")]
        public string GrandparentRatingKey { get; set; }
        [XmlAttribute(AttributeName = "grandparentKey")]
        public string GrandparentKey { get; set; }
        [XmlAttribute(AttributeName = "parentKey")]
        public string ParentKey { get; set; }
        [XmlAttribute(AttributeName = "grandparentTitle")]
        public string GrandparentTitle { get; set; }
        [XmlAttribute(AttributeName = "index")]
        public string Index { get; set; }
        [XmlAttribute(AttributeName = "parentIndex")]
        public string ParentIndex { get; set; }
        [XmlAttribute(AttributeName = "parentThumb")]
        public string ParentThumb { get; set; }
        [XmlAttribute(AttributeName = "grandparentThumb")]
        public string GrandparentThumb { get; set; }
        [XmlAttribute(AttributeName = "grandparentArt")]
        public string GrandparentArt { get; set; }
        [XmlAttribute(AttributeName = "viewCount")]
        public string ViewCount { get; set; }
        [XmlAttribute(AttributeName = "lastViewedAt")]
        public string LastViewedAt { get; set; }
        [XmlAttribute(AttributeName = "grandparentTheme")]
        public string GrandparentTheme { get; set; }
    }

    [XmlRoot(ElementName = "Provider")]
    public class Provider
    {
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "Directory")]
    public class Directory1
    {
        public string ProviderId { get; set; }
        [XmlAttribute(AttributeName = "guid")]
        public string Guid { get; set; }
        [XmlElement(ElementName = "Genre")]
        public List<Genre> Genre { get; set; }
        [XmlElement(ElementName = "Role")]
        public List<Role> Role { get; set; }
        [XmlAttribute(AttributeName = "allowSync")]
        public string AllowSync { get; set; }
        [XmlAttribute(AttributeName = "librarySectionID")]
        public string LibrarySectionID { get; set; }
        [XmlAttribute(AttributeName = "librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }
        [XmlAttribute(AttributeName = "librarySectionUUID")]
        public string LibrarySectionUUID { get; set; }
        [XmlAttribute(AttributeName = "personal")]
        public string Personal { get; set; }
        [XmlAttribute(AttributeName = "sourceTitle")]
        public string SourceTitle { get; set; }
        [XmlAttribute(AttributeName = "ratingKey")]
        public string RatingKey { get; set; }
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlAttribute(AttributeName = "studio")]
        public string Studio { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
        [XmlAttribute(AttributeName = "contentRating")]
        public string ContentRating { get; set; }
        [XmlAttribute(AttributeName = "summary")]
        public string Summary { get; set; }
        [XmlAttribute(AttributeName = "index")]
        public string Index { get; set; }
        [XmlAttribute(AttributeName = "rating")]
        public string Rating { get; set; }
        [XmlAttribute(AttributeName = "viewCount")]
        public string ViewCount { get; set; }
        [XmlAttribute(AttributeName = "lastViewedAt")]
        public string LastViewedAt { get; set; }
        [XmlAttribute(AttributeName = "year")]
        public string Year { get; set; }
        [XmlAttribute(AttributeName = "thumb")]
        public string Thumb { get; set; }
        [XmlAttribute(AttributeName = "art")]
        public string Art { get; set; }
        [XmlAttribute(AttributeName = "banner")]
        public string Banner { get; set; }
        [XmlAttribute(AttributeName = "theme")]
        public string Theme { get; set; }
        [XmlAttribute(AttributeName = "duration")]
        public string Duration { get; set; }
        [XmlAttribute(AttributeName = "originallyAvailableAt")]
        public string OriginallyAvailableAt { get; set; }
        [XmlAttribute(AttributeName = "leafCount")]
        public string LeafCount { get; set; }
        [XmlAttribute(AttributeName = "viewedLeafCount")]
        public string ViewedLeafCount { get; set; }
        [XmlAttribute(AttributeName = "childCount")]
        public string ChildCount { get; set; }
        [XmlAttribute(AttributeName = "addedAt")]
        public string AddedAt { get; set; }
        [XmlAttribute(AttributeName = "updatedAt")]
        public string UpdatedAt { get; set; }
        [XmlAttribute(AttributeName = "parentTitle")]
        public string ParentTitle { get; set; }
        public List<Directory1> Seasons { get; set; }
    }


    [XmlRoot(ElementName = "MediaContainer")]
    public class PlexSearch
    {

        [XmlElement(ElementName = "Directory")]
        public List<Directory1> Directory { get; set; }
        [XmlElement(ElementName = "Video")]
        public List<Video> Video { get; set; }
        [XmlElement(ElementName = "Provider")]
        public List<Provider> Provider { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
        [XmlAttribute(AttributeName = "totalSize")]
        public string TotalSize { get; set; }
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier { get; set; }
        [XmlAttribute(AttributeName = "mediaTagPrefix")]
        public string MediaTagPrefix { get; set; }
        [XmlAttribute(AttributeName = "mediaTagVersion")]
        public string MediaTagVersion { get; set; }
    }
}