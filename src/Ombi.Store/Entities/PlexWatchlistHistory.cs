using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table(nameof(PlexWatchlistHistory))]
    public class PlexWatchlistHistory : Entity
    {
        public string TmdbId { get; set; }
        public string UserId { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
