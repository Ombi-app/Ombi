using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Ombi.Store.Entities
{
    public abstract class MediaServerContent: Entity, IMediaServerContent
    {
        public string Title { get; set; }
        public string ImdbId { get; set; }
        public string TvDbId { get; set; }
        public string TheMovieDbId { get; set; }
        public MediaType Type { get; set; }
        public string Quality { get; set; }
        public string Url { get; set; }
        
        public ICollection<IMediaServerEpisode> Episodes { get; set; }

        public DateTime AddedAt { get; set; }

        [NotMapped]
        public bool HasImdb => !string.IsNullOrEmpty(ImdbId);

        [NotMapped]
        public bool HasTvDb => !string.IsNullOrEmpty(TvDbId);

        [NotMapped]
        public bool HasTheMovieDb => !string.IsNullOrEmpty(TheMovieDbId);

        [NotMapped]
        public abstract RecentlyAddedType RecentlyAddedType { get; }
    }

    public abstract class MediaServerEpisode: Entity, IMediaServerEpisode
    {
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string Title { get; set; }

        public IMediaServerContent Series { get; set; }

        public abstract IMediaServerContent SeriesIsIn(ICollection<IMediaServerContent> content);
        public bool IsIn(IMediaServerContent content)
        {
            return content.Episodes.Any(x => x.SeasonNumber == this.SeasonNumber && x.EpisodeNumber == this.EpisodeNumber);
        }
    }
}