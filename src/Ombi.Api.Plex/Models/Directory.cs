using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ombi.Api.Plex.Models
{
    public class Directory
    {
        public Directory()
        {
            seasons = new List<Directory>();
        }
        public bool allowSync { get; set; }
        public string art { get; set; }
        public string composite { get; set; }
        public bool filters { get; set; }
        public bool refreshing { get; set; }
        public string thumb { get; set; }
        public string key { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string agent { get; set; }
        public string scanner { get; set; }
        public string language { get; set; }
        public string uuid { get; set; }
        public int updatedAt { get; set; }
        public int createdAt { get; set; }
        public Location[] Location { get; set; }
        public string providerId { get; set; }
        public string guid { get; set; }
        public List<Genre> genre { get; set; }
        public List<Role> role { get; set; }
        public string librarySectionID { get; set; }
        public string librarySectionTitle { get; set; }
        public string librarySectionUUID { get; set; }
        public string personal { get; set; }
        public string sourceTitle { get; set; }
        public string ratingKey { get; set; }
        public string studio { get; set; }
        public List<Directory> seasons { get; set; }
    }
}