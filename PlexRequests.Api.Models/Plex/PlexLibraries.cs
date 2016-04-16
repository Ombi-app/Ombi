using System.Collections.Generic;
using System.Xml.Serialization;

namespace PlexRequests.Api.Models.Plex
{
    [XmlRoot(ElementName = "MediaContainer")]
    public class PlexLibraries
    {
        [XmlElement(ElementName = "Directory")]
        public List<Directory> Directories { get; set; }
    }

    [XmlRoot(ElementName = "Location")]
    public partial class Location
    {
        [XmlElement(ElementName = "id")]
        public int id { get; set; }
        [XmlElement(ElementName = "path")]
        public string path { get; set; }
    }

}
