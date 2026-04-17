using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table(nameof(PlexWatchlistUserStatus))]
    public class PlexWatchlistUserStatus : Entity
    {
        [MaxLength(128)]
        public string UserId { get; set; }
        public int SyncStatus { get; set; }
        public DateTime LastSyncedAt { get; set; }
    }
}
