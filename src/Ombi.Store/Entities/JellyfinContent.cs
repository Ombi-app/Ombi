#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: JellyfinContent.cs
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
    [Table("JellyfinContent")]
    public class JellyfinContent : Entity
    {
        public string Title { get; set; }

        /// <summary>
        /// OBSOLETE, Cannot delete due to DB migration issues with SQLite
        /// </summary>
        public string ProviderId { get; set; }
        public string JellyfinId { get; set; }
        public JellyfinMediaType Type { get; set; }
        public DateTime AddedAt { get; set; }

        public string ImdbId { get; set; }
        public string TheMovieDbId { get; set; }
        public string TvDbId { get; set; }

        public string Url { get; set; }

        public ICollection<JellyfinEpisode> Episodes { get; set; }

        [NotMapped]
        public bool HasImdb => !string.IsNullOrEmpty(ImdbId);

        [NotMapped]
        public bool HasTvDb => !string.IsNullOrEmpty(TvDbId);

        [NotMapped]
        public bool HasTheMovieDb => !string.IsNullOrEmpty(TheMovieDbId);
    }

    public enum JellyfinMediaType
    {
        Movie = 0,
        Series = 1,
        Music = 2
    }
}
