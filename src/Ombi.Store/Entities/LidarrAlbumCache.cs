using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("LidarrAlbumCache")]
    public class LidarrAlbumCache : Entity
    {
        public int ArtistId { get; set; }
        public string ForeignAlbumId { get; set; }
        public int TrackCount { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool Monitored { get; set; }
        public string Title { get; set; }
        public decimal PercentOfTracks { get; set; }
        public DateTime AddedAt { get; set; }

        [NotMapped]
        public bool PartiallyAvailable => PercentOfTracks != 100 && PercentOfTracks > 0;
        [NotMapped]
        public bool FullyAvailable => PercentOfTracks == 100;
    }
}