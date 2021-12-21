using System.Collections.Generic;

namespace Ombi.Api.Plex.Models
{
    public class Mediacontainer
    {
        public int size { get; set; }
        public int totalSize { get; set; }
        public string title1 { get; set; }
        public List<Directory> Directory { get; set; }
        public string art { get; set; }
        public int librarySectionID { get; set; }
        public string librarySectionTitle { get; set; }
        public string librarySectionUUID { get; set; }
        public bool nocache { get; set; }
        public string thumb { get; set; }
        public string title2 { get; set; }
        public string viewGroup { get; set; }
        public int viewMode { get; set; }
        public Metadata[] Metadata { get; set; }

    }
}