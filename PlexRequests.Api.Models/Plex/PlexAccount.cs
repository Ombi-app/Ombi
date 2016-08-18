using System.Xml.Serialization;

namespace PlexRequests.Api.Models.Plex
{
    [XmlRoot(ElementName = "user")]
    public class PlexAccount
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "username")]
        public string Username { get; set; }
        [XmlAttribute(AttributeName = "email")]
        public string Email { get; set; }
        [XmlAttribute(AttributeName = "authenticationToken")]
        public string AuthToken { get; set; }
    }
}
