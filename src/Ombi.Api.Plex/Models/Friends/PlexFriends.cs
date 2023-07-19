using System.Xml.Serialization;

namespace Ombi.Api.Plex.Models.Friends
{
    [XmlRoot(ElementName = "User")]
    public class UserFriends
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        /// <summary>
        /// Title is for Home Users only
        /// </summary>
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
        [XmlAttribute(AttributeName = "username")]
        public string Username { get; set; }
        [XmlAttribute(AttributeName = "email")]
        public string Email { get; set; }
        [XmlAttribute(AttributeName = "home")]
        public bool HomeUser { get; set; }
    }

    [XmlRoot(ElementName = "MediaContainer")]
    public class PlexUsers
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