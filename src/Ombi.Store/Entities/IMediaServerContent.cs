using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Ombi.Store.Repository;

namespace Ombi.Store.Entities
{
    public interface IMediaServerContent: IEntity
    {
        public string Title { get; set; }
        public string ImdbId { get; set; }
        public string TvDbId { get; set; }
        public string TheMovieDbId { get; set; }
        public MediaType Type { get; set; }
        public IMediaServerContentRepositoryLight Repository { get; }

        public string Url { get; set; }
        public string GetExternalUrl();
        
        public ICollection<IMediaServerEpisode> Episodes { get; set; }

        public DateTime AddedAt { get; set; }

        [NotMapped]
        public bool HasImdb => !string.IsNullOrEmpty(ImdbId);

        [NotMapped]
        public bool HasTvDb => !string.IsNullOrEmpty(TvDbId);

        [NotMapped]
        public bool HasTheMovieDb => !string.IsNullOrEmpty(TheMovieDbId);
    }

    public interface IMediaServerEpisode
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
        public IMediaServerContent SeriesIsIn(List<IMediaServerContent> content);
        public bool IsIn(IMediaServerContent content);
    }

    public enum MediaType
    {
        Movie = 0,
        Series = 1,
        Music = 2,
        Episode = 3
    }
}