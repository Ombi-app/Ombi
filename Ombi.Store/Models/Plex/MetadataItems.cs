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
using Dapper.Contrib.Extensions;

namespace Ombi.Store.Models.Plex
{
    [Table("metadata_items")]
    public class MetadataItems
    {
        [Key]
        public int id { get; set; }
        
        public int library_section_id { get; set; }
        public int parent_id { get; set; }
        public int metadata_type { get; set; }
        public string guid { get; set; }
        public int media_item_count { get; set; }
        public string title { get; set; }
        public string title_sort { get; set; }
        public string original_title { get; set; }
        public string studio { get; set; }
        public float rating { get; set; }
        public int rating_count { get; set; }
        public string tagline { get; set; }
        public string summary { get; set; }
        public string trivia { get; set; }
        public string quotes { get; set; }
        public string content_rating { get; set; }
        public int content_rating_age { get; set; }
        public int Index { get; set; }
        public string tags_genre { get; set; }
        // SKIP Until Date Times
        
        public DateTime originally_available_at { get; set; }
        public DateTime available_at { get; set; }
        public DateTime expires_at { get; set; }
        // Skip RefreshedAt and Year
        public DateTime added_at { get; set; }
        public string SeriesTitle { get; set; } // Only used in a custom query for the TV Shows
    }
}