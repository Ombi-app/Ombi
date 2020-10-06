using System;

namespace Ombi.Core.Models
{
    public class RecentlyAddedTvModel
    {
        public int Id { get; set; }
        public string Title { get; set; } // Series Title
        public string Overview { get; set; }
        public string ImdbId { get; set; }
        public string TvDbId { get; set; }
        public string TheMovieDbId { get; set; }
        public string ReleaseYear { get; set; }
        public DateTime AddedAt { get; set; }
        public string Quality { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public DateTime Aired { get; set; }
    }
}