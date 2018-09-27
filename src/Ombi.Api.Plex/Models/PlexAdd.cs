using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ombi.Api.Plex.Models
{
    [XmlRoot(ElementName = "Section")]
    public class Section
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "shared")]
        public string Shared { get; set; }
    }

    [XmlRoot(ElementName = "SharedServer")]
    public class SharedServer
    {
        [XmlElement(ElementName = "Section")]
        public List<Section> Section { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "username")]
        public string Username { get; set; }
        [XmlAttribute(AttributeName = "email")]
        public string Email { get; set; }
        [XmlAttribute(AttributeName = "userID")]
        public string UserID { get; set; }
        [XmlAttribute(AttributeName = "accessToken")]
        public string AccessToken { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "acceptedAt")]
        public string AcceptedAt { get; set; }
        [XmlAttribute(AttributeName = "invitedAt")]
        public string InvitedAt { get; set; }
        [XmlAttribute(AttributeName = "allowSync")]
        public string AllowSync { get; set; }
        [XmlAttribute(AttributeName = "allowCameraUpload")]
        public string AllowCameraUpload { get; set; }
        [XmlAttribute(AttributeName = "allowChannels")]
        public string AllowChannels { get; set; }
        [XmlAttribute(AttributeName = "allowTuners")]
        public string AllowTuners { get; set; }
        [XmlAttribute(AttributeName = "owned")]
        public string Owned { get; set; }
    }

    [XmlRoot(ElementName = "MediaContainer")]
    public class PlexAdd
    {
        [XmlElement(ElementName = "SharedServer")]
        public SharedServer SharedServer { get; set; }
        [XmlAttribute(AttributeName = "friendlyName")]
        public string FriendlyName { get; set; }
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier { get; set; }
        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
    }

    [XmlRoot(ElementName = "Response")]
    public class AddUserError
    {
        [XmlAttribute(AttributeName = "code")]
        public string Code { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
    }

    public class PlexAddWrapper
    {
        public PlexAdd Add { get; set; }
        public AddUserError Error { get; set; }
        public bool HasError => Error != null;
    }
}