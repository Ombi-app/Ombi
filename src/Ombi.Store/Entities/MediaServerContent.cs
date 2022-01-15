using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Ombi.Store.Repository;

namespace Ombi.Store.Entities
{
    public abstract class MediaServerContent: Entity, IMediaServerContent
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
        
        [NotMapped] //TODO: instantiate this variable upon read // something in ExternalContext.cs?
        public IMediaServerContentRepositoryLight Repository { get; set; }
    }

    public abstract class MediaServerEpisode: Entity, IMediaServerEpisode
    {
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string Title { get; set; }

        public IMediaServerContent Series { get; set; }

        public abstract IMediaServerContent SeriesIsIn(List<IMediaServerContent> content);
        public abstract bool IsIn(IMediaServerContent content);
    }
}