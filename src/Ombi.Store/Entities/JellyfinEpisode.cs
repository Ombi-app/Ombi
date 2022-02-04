﻿#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: JellyfinEpisode.cs
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
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ombi.Store.Entities
{
    [Table("JellyfinEpisode")]
    public class JellyfinEpisode : MediaServerEpisode
    {
        public string JellyfinId { get; set; }
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
        public JellyfinContent JellyfinSeries
        {
            get => (JellyfinContent)Series;
            set => Series = value;
        }
        
        public override IMediaServerContent SeriesIsIn(ICollection<IMediaServerContent> content)
        {
            return content.OfType<JellyfinContent>().FirstOrDefault(
                x => x.JellyfinId == this.JellyfinSeries.JellyfinId);
        }
    }
}
