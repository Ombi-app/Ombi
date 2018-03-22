using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("RecentlyAddedLog")]
    public class RecentlyAddedLog : Entity
    {
        public RecentlyAddedType Type { get; set; }
        public int ContentId { get; set; } // This is dependant on the type
        public DateTime AddedAt { get; set; }
    }

    public enum RecentlyAddedType
    {
        Plex,
        Emby
    }
}