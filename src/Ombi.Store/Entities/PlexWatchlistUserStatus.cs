using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table(nameof(PlexWatchlistUserStatus))]
    public class PlexWatchlistUserStatus : Entity
    {
        public string UserId { get; set; }
        public int SyncStatus { get; set; }
        public DateTime LastSyncedAt { get; set; }
    }
}
