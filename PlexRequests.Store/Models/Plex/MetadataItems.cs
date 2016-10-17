#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: MetadataItems.cs
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

using System;
using System.Data.Linq.Mapping;

namespace PlexRequests.Store.Models.Plex
{
    [Table(Name = "metadata_items")]
    public class MetadataItems
    {
        [Column(IsPrimaryKey = true)]
        public int Id { get; set; }

        [Column(Name = "library_section_id")]
        public int LibrarySectionId { get; set; }

        [Column(Name = "parent_id")]
        public int ParentId { get; set; }

        [Column(Name = "metadata_type")]
        public int MetadataType { get; set; }

        [Column(Name = "guid")]
        public string Guid { get; set; }

        [Column(Name = "media_item_count")]
        public int MediaItemCount { get; set; }

        [Column(Name = "title")]
        public string Title { get; set; }

        [Column(Name = "title_sort")]
        public string TitleSort { get; set; }

        [Column(Name = "OriginalTitle")]
        public string OriginalTitle { get; set; }

        [Column(Name = "studio")]
        public string Studio { get; set; }
        [Column(Name = "rating")]
        public float Rating { get; set; }
        [Column(Name = "rating_count")]
        public int RatingCount { get; set; }
        [Column(Name = "tagline")]
        public string Tagline { get; set; }
        [Column(Name = "summary")]
        public string Summary { get; set; }
        [Column(Name = "trivia")]
        public string Trivia { get; set; }
        [Column(Name = "quotes")]
        public string Quotes { get; set; }
        [Column(Name = "content_rating")]
        public string ContentRating { get; set; }
        [Column(Name = "content_rating_age")]
        public int ContentRatingAge { get; set; }
        [Column(Name = "Index")]
        public int Index { get; set; }
        // SKIP Until Date Times

        [Column(Name = "originally_available_at")]
        public DateTime OriginallyAvailableAt { get; set; }
        [Column(Name = "available_at")]
        public DateTime AvailableAt { get; set; }
        [Column(Name = "expires_at")]
        public DateTime ExpiresAt { get; set; }
        // Skip RefreshedAt and Year
        [Column(Name = "added_at")]
        public DateTime AddedAt { get; set; }
    }
}