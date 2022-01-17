#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: EmbyEpisode.cs
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
using System.Linq;

namespace Ombi.Store.Entities
{
    [Table("EmbyEpisode")]
    public class EmbyEpisode : MediaServerEpisode
    {
        public string EmbyId { get; set; }
        public string ParentId { get; set; }
        /// <summary>
        /// NOT USED
        /// </summary>
        public string ProviderId { get; set; }
        public DateTime AddedAt { get; set; }
        public string TvDbId { get; set; }
        public string ImdbId { get; set; }
        public string TheMovieDbId { get; set; }
        [NotMapped]
        public EmbyContent EmbySeries
        {
            get => (EmbyContent)Series;
            set => Series = value;
        }

        public override IMediaServerContent SeriesIsIn(ICollection<IMediaServerContent> content)
        {
            return content.OfType<EmbyContent>().FirstOrDefault(
                x => x.EmbyId == this.EmbySeries.EmbyId);
        }

        public override bool IsIn(IMediaServerContent content)
        {
            return content.Episodes.Cast<EmbyEpisode>().Any(x => x.EmbyId == this.EmbyId);
        }

    }
}