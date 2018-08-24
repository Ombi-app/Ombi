using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("LidarrArtistCache")]
    public class LidarrArtistCache : Entity
    {
        public int ArtistId { get; set; }
        public string ArtistName { get; set; }
        public string ForeignArtistId { get; set; }
        public bool Monitored { get; set; }
    }
}