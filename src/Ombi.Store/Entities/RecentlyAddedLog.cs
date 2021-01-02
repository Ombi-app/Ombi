using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("RecentlyAddedLog")]
    public class RecentlyAddedLog : Entity
    {
        public RecentlyAddedType Type { get; set; }
        public ContentType ContentType { get; set; }
        public int ContentId { get; set; } // This is dependant on the type, it's either TMDBID or TVDBID
        public int? EpisodeNumber { get; set; }
        public int? SeasonNumber { get; set; }
        public string AlbumId { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public enum RecentlyAddedType
    {
        Plex = 0,
        Emby = 1,
        Lidarr = 2,
        Jellyfin = 3
    }

    public enum ContentType
    {
        Parent = 0,
        Episode = 1,
        Album = 2,
    }
}
