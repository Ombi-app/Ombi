namespace Ombi.Api.Plex.Models
{

    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    [XmlRoot(ElementName = "Section")]
    public class SectionLite
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
    }

    [XmlRoot(ElementName = "Server")]
    public class ServerLib
    {
        [XmlElement(ElementName = "Section")]
        public List<SectionLite> Section { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "address")]
        public string Address { get; set; }
        [XmlAttribute(AttributeName = "port")]
        public string Port { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "scheme")]
        public string Scheme { get; set; }
        [XmlAttribute(AttributeName = "host")]
        public string Host { get; set; }
        [XmlAttribute(AttributeName = "localAddresses")]
        public string LocalAddresses { get; set; }
        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }
        [XmlAttribute(AttributeName = "createdAt")]
        public string CreatedAt { get; set; }
        [XmlAttribute(AttributeName = "updatedAt")]
        public string UpdatedAt { get; set; }
        [XmlAttribute(AttributeName = "owned")]
        public string Owned { get; set; }
        [XmlAttribute(AttributeName = "synced")]
        public string Synced { get; set; }
    }

    [XmlRoot(ElementName = "MediaContainer")]
    public class PlexLibrariesForMachineId
    {
        [XmlElement(ElementName = "Server")]
        public ServerLib Server { get; set; }
        [XmlAttribute(AttributeName = "friendlyName")]
        public string FriendlyName { get; set; }
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier { get; set; }
        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
    }
}