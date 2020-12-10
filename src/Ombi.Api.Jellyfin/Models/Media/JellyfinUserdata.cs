using System;

namespace Ombi.Api.Jellyfin.Models.Movie
{
    public class JellyfinUserdata
    {
        public double PlaybackPositionTicks { get; set; }
        public int PlayCount { get; set; }
        public bool IsFavorite { get; set; }
        public bool Played { get; set; }
        public string Key { get; set; }
        public DateTime LastPlayedDate { get; set; }
        public int UnplayedItemCount { get; set; }
    }
}