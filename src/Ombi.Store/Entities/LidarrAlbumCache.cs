using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    /// <summary>
    /// Cached Lidarr album metadata synced from the Lidarr API for availability checks.
    /// Uses <see cref="LongEntity"/> because this table is bulk-deleted and re-inserted on every sync.
    /// </summary>
    [Table("LidarrAlbumCache")]
    public class LidarrAlbumCache : LongEntity
    {
        public int ArtistId { get; set; }
        public string ForeignAlbumId { get; set; }
        public int TrackCount { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool Monitored { get; set; }
        public string Title { get; set; }
        public decimal PercentOfTracks { get; set; }
        public DateTime AddedAt { get; set; }

        /// <summary>
        /// True when some but not all tracks in the album are available locally.
        /// </summary>
        [NotMapped]
        public bool PartiallyAvailable => PercentOfTracks != 100 && PercentOfTracks > 0;
        /// <summary>
        /// True when every track in the album is available locally.
        /// </summary>
        [NotMapped]
        public bool FullyAvailable => PercentOfTracks == 100;
    }
}