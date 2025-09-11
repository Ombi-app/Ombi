#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: Device.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Ombi.Api.Plex.Models.Server
{
    [XmlRoot(ElementName = "Device")]
    public class Device
    {
        [XmlElement(ElementName = "Connection")]
        public List<Connection> Connections { get; set; }
        
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        
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
        
        [XmlAttribute(AttributeName = "createdAt")]
        public string CreatedAt { get; set; }
        
        [XmlAttribute(AttributeName = "lastSeenAt")]
        public string LastSeenAt { get; set; }
        
        [XmlAttribute(AttributeName = "provides")]
        public string Provides { get; set; }
        
        [XmlAttribute(AttributeName = "owned")]
        public string Owned { get; set; }
        
        [XmlAttribute(AttributeName = "accessToken")]
        public string AccessToken { get; set; }
        
        [XmlAttribute(AttributeName = "searchEnabled")]
        public string SearchEnabled { get; set; }
        
        [XmlAttribute(AttributeName = "publicAddress")]
        public string PublicAddress { get; set; }
        
        [XmlAttribute(AttributeName = "httpsRequired")]
        public string HttpsRequired { get; set; }
        
        [XmlAttribute(AttributeName = "synced")]
        public string Synced { get; set; }
        
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
        
        [XmlAttribute(AttributeName = "ownerId")]
        public string OwnerId { get; set; }
        
        [XmlAttribute(AttributeName = "home")]
        public string Home { get; set; }
        
        [XmlAttribute(AttributeName = "sourceTitle")]
        public string SourceTitle { get; set; }
        
        // Properties for backward compatibility with existing code
        // These will be populated from the first local connection
        [XmlIgnore]
        public string LocalAddresses => Connections?.Where(c => c.Local == "1").Select(c => c.Address).FirstOrDefault() ?? string.Empty;
        
        [XmlIgnore]
        public string MachineIdentifier => ClientIdentifier;
        
        [XmlIgnore]
        public string Port => Connections?.Where(c => c.Local == "1").Select(c => c.Port).FirstOrDefault() ?? "32400";
        
        [XmlIgnore]
        public string Scheme => Connections?.Where(c => c.Local == "1").Select(c => c.Protocol).FirstOrDefault() ?? "http";
    }
}
