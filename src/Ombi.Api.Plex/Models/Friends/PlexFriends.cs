using System.Xml.Serialization;

namespace Ombi.Api.Plex.Models.Friends
{
    [XmlRoot(ElementName = "Server")]
    public class Server
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "serverId")]
        public string ServerId { get; set; }
        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "lastSeenAt")]
        public string LastSeenAt { get; set; }
        [XmlAttribute(AttributeName = "numLibraries")]
        public string NumLibraries { get; set; }
        [XmlAttribute(AttributeName = "owned")]
        public string Owned { get; set; }
    }

    [XmlRoot(ElementName = "User")]
    public class UserFriends
    {
        [XmlElement(ElementName = "Server")]
        public Server Server { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
        [XmlAttribute(AttributeName = "username")]
        public string Username { get; set; }
        [XmlAttribute(AttributeName = "email")]
        public string Email { get; set; }
        [XmlAttribute(AttributeName = "recommendationsPlaylistId")]
        public string RecommendationsPlaylistId { get; set; }
        [XmlAttribute(AttributeName = "thumb")]
        public string Thumb { get; set; }
    }

    [XmlRoot(ElementName = "MediaContainer")]
    public class PlexFriends
    {
        [XmlElement(ElementName = "User")]
        public UserFriends[] User { get; set; }
        [XmlAttribute(AttributeName = "friendlyName")]
        public string FriendlyName { get; set; }
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier { get; set; }
        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }
        [XmlAttribute(AttributeName = "totalSize")]
        public string TotalSize { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
    }
}