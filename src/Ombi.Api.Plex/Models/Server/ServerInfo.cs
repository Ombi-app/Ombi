using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ombi.Api.Plex.Models.Server
{
    [XmlRoot(ElementName = "Server")]
    public class ServerInfo
    {
        [XmlElement(ElementName = "Connection")]
        public List<Connection> Connections { get; set; }
        
        [XmlAttribute(AttributeName = "accessToken")]
        public string AccessToken { get; set; }
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
        [XmlAttribute(AttributeName = "sourceTitle")]
        public string SourceTitle { get; set; }
        [XmlAttribute(AttributeName = "ownerId")]
        public string OwnerId { get; set; }
        [XmlAttribute(AttributeName = "home")]
        public string Home { get; set; }
        
        // New attributes from Device elements
        [XmlAttribute(AttributeName = "product")]
        public string Product { get; set; }
        
        [XmlAttribute(AttributeName = "productVersion")]
        public string ProductVersion { get; set; }
        
        [XmlAttribute(AttributeName = "platform")]
        public string Platform { get; set; }
        
        [XmlAttribute(AttributeName = "platformVersion")]
        public string PlatformVersion { get; set; }
        
        [XmlAttribute(AttributeName = "device")]
        public string DeviceType { get; set; }
        
        [XmlAttribute(AttributeName = "clientIdentifier")]
        public string ClientIdentifier { get; set; }
        
        [XmlAttribute(AttributeName = "lastSeenAt")]
        public string LastSeenAt { get; set; }
        
        [XmlAttribute(AttributeName = "provides")]
        public string Provides { get; set; }
        
        [XmlAttribute(AttributeName = "searchEnabled")]
        public string SearchEnabled { get; set; }
        
        [XmlAttribute(AttributeName = "publicAddress")]
        public string PublicAddress { get; set; }
        
        [XmlAttribute(AttributeName = "httpsRequired")]
        public string HttpsRequired { get; set; }
        
        [XmlAttribute(AttributeName = "relay")]
        public string Relay { get; set; }
        
        [XmlAttribute(AttributeName = "dnsRebindingProtection")]
        public string DnsRebindingProtection { get; set; }
        
        [XmlAttribute(AttributeName = "natLoopbackSupported")]
        public string NatLoopbackSupported { get; set; }
        
        [XmlAttribute(AttributeName = "publicAddressMatches")]
        public string PublicAddressMatches { get; set; }
        
        [XmlAttribute(AttributeName = "presence")]
        public string Presence { get; set; }
    }
}