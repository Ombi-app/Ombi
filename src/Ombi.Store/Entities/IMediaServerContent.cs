using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    public interface IMediaServerContent: IEntity
    {
        public string Title { get; set; }
        public string ImdbId { get; set; }
        public string TvDbId { get; set; }
        public string TheMovieDbId { get; set; }
        public MediaType Type { get; set; }
        public RecentlyAddedType RecentlyAddedType{ get; }

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

    public interface IMediaServerEpisode: IBaseMediaServerEpisode
    {
        public string Title { get; set; }
        /// <summary>
        /// The Season key
        /// </summary>
        /// <value>
        /// The parent key.
        /// </value>


        public IMediaServerContent Series { get; set; }
        public IMediaServerContent SeriesIsIn(ICollection<IMediaServerContent> content);
        public bool IsIn(IMediaServerContent content);
    }
    
    public interface IBaseMediaServerEpisode
    {
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
    }

    public enum MediaType
    {
        Movie = 0,
        Series = 1,
        Music = 2,
        Episode = 3
    }
}