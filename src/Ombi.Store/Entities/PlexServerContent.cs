#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: PlexContent.cs
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("PlexServerContent")]
    public class PlexServerContent : Entity
    {
        public string Title { get; set; }
        public string ReleaseYear { get; set; }
        public string ImdbId { get; set; }
        public string TvDbId { get; set; }
        public string TheMovieDbId { get; set; }
        public PlexMediaTypeEntity Type { get; set; }

        public string Url { get; set; }
        
        public ICollection<PlexEpisode> Episodes { get; set; }
        public ICollection<PlexSeasonsContent> Seasons { get; set; }

        /// <summary>
        /// Plex's internal ID for this item
        /// </summary>
        public int Key { get; set; }
        public DateTime AddedAt { get; set; }
        public string Quality { get; set; }

        [NotMapped]
        public bool HasImdb => !string.IsNullOrEmpty(ImdbId);

        [NotMapped]
        public bool HasTvDb => !string.IsNullOrEmpty(TvDbId);

        [NotMapped]
        public bool HasTheMovieDb => !string.IsNullOrEmpty(TheMovieDbId);
    }

    [Table("PlexSeasonsContent")]
    public class PlexSeasonsContent : Entity
    {
        public int PlexContentId { get; set; }
        public int SeasonNumber { get; set; }
        public int SeasonKey { get; set; }
        public int ParentKey { get; set; }
    }

    public enum PlexMediaTypeEntity
    {
        Movie = 0,
        Show = 1
    }
}