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

        /// <summary>
        /// The last time this title was confirmed to still be on the user's Plex watchlist.
        /// Used to debounce history pruning so a single flaky/ambiguous sync can't wipe a row
        /// and cause the title to be re-requested (issue #5427). Null on legacy rows created
        /// before this tracking was introduced; these remain immune to pruning until they are
        /// next confirmed on the watchlist and assigned a timestamp.
        /// </summary>
        public DateTime? LastSeenAt { get; set; }
    }
}
