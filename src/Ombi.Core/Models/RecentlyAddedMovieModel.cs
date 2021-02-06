using System;

namespace Ombi.Core.Models
{
    public class RecentlyAddedMovieModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string ImdbId { get; set; }
        public string TvDbId { get; set; }
        public string TheMovieDbId { get; set; }
        public string ReleaseYear { get; set; }
        public DateTime AddedAt { get; set; }
        public string Quality { get; set; }
    }

    public enum RecentlyAddedType
    {
        Plex,
        Emby,
        Jellyfin
    }
}
