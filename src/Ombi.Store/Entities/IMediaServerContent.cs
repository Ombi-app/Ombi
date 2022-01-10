using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    public interface IMediaServerContent<IMediaServerEpisode>
    {
        public string Title { get; set; }
        public string ImdbId { get; set; }
        public string TvDbId { get; set; }
        public string TheMovieDbId { get; set; }
        public MediaType Type { get; set; }

        public string Url { get; set; }
        
        public ICollection<IMediaServerEpisode> Episodes { get; set; }

        public DateTime AddedAt { get; set; }

        [NotMapped]
        public bool HasImdb => !string.IsNullOrEmpty(ImdbId);

        [NotMapped]
        public bool HasTvDb => !string.IsNullOrEmpty(TvDbId);

        [NotMapped]
        public bool HasTheMovieDb => !string.IsNullOrEmpty(TheMovieDbId);
    }

    public interface IMediaServerEpisode<IMediaServerContent>
    {
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// The Season key
        /// </summary>
        /// <value>
        /// The parent key.
        /// </value>


        public IMediaServerContent Series { get; set; }
    }

    public enum MediaType
    {
        Movie = 0,
        Series = 1,
        Music = 2,
        Episode = 3
    }
}