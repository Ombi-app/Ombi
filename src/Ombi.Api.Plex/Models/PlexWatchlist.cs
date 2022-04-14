using System.Collections.Generic;

namespace Ombi.Api.Plex.Models
{
    public class PlexWatchlist
    {
        public string librarySectionID { get; set; }
        public string librarySectionTitle { get; set; }
        public int offset { get; set; }
        public int totalSize { get; set; }
        public int size { get; set; }
        public List<Metadata> Metadata { get; set; } = new List<Metadata>();
    }
}