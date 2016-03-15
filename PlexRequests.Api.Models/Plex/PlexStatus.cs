using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PlexRequests.Api.Models.Plex
{
    [XmlRoot(ElementName = "Directory")]
    public class Directory
    {
        [XmlAttribute(AttributeName = "count")]
        public string Count { get; set; }
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
    }

    [XmlRoot(ElementName = "MediaContainer")]
    public class PlexStatus
    {
        [XmlElement(ElementName = "Directory")]
        public List<Directory> Directory { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
        [XmlAttribute(AttributeName = "allowCameraUpload")]
        public string AllowCameraUpload { get; set; }
        [XmlAttribute(AttributeName = "allowChannelAccess")]
        public string AllowChannelAccess { get; set; }
        [XmlAttribute(AttributeName = "allowMediaDeletion")]
        public string AllowMediaDeletion { get; set; }
        [XmlAttribute(AttributeName = "allowSync")]
        public string AllowSync { get; set; }
        [XmlAttribute(AttributeName = "backgroundProcessing")]
        public string BackgroundProcessing { get; set; }
        [XmlAttribute(AttributeName = "certificate")]
        public string Certificate { get; set; }
        [XmlAttribute(AttributeName = "companionProxy")]
        public string CompanionProxy { get; set; }
        [XmlAttribute(AttributeName = "friendlyName")]
        public string FriendlyName { get; set; }
        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }
        [XmlAttribute(AttributeName = "multiuser")]
        public string Multiuser { get; set; }
        [XmlAttribute(AttributeName = "myPlex")]
        public string MyPlex { get; set; }
        [XmlAttribute(AttributeName = "myPlexMappingState")]
        public string MyPlexMappingState { get; set; }
        [XmlAttribute(AttributeName = "myPlexSigninState")]
        public string MyPlexSigninState { get; set; }
        [XmlAttribute(AttributeName = "myPlexSubscription")]
        public string MyPlexSubscription { get; set; }
        [XmlAttribute(AttributeName = "myPlexUsername")]
        public string MyPlexUsername { get; set; }
        [XmlAttribute(AttributeName = "platform")]
        public string Platform { get; set; }
        [XmlAttribute(AttributeName = "platformVersion")]
        public string PlatformVersion { get; set; }
        [XmlAttribute(AttributeName = "requestParametersInCookie")]
        public string RequestParametersInCookie { get; set; }
        [XmlAttribute(AttributeName = "sync")]
        public string Sync { get; set; }
        [XmlAttribute(AttributeName = "transcoderActiveVideoSessions")]
        public string TranscoderActiveVideoSessions { get; set; }
        [XmlAttribute(AttributeName = "transcoderAudio")]
        public string TranscoderAudio { get; set; }
        [XmlAttribute(AttributeName = "transcoderLyrics")]
        public string TranscoderLyrics { get; set; }
        [XmlAttribute(AttributeName = "transcoderPhoto")]
        public string TranscoderPhoto { get; set; }
        [XmlAttribute(AttributeName = "transcoderSubtitles")]
        public string TranscoderSubtitles { get; set; }
        [XmlAttribute(AttributeName = "transcoderVideo")]
        public string TranscoderVideo { get; set; }
        [XmlAttribute(AttributeName = "transcoderVideoBitrates")]
        public string TranscoderVideoBitrates { get; set; }
        [XmlAttribute(AttributeName = "transcoderVideoQualities")]
        public string TranscoderVideoQualities { get; set; }
        [XmlAttribute(AttributeName = "transcoderVideoResolutions")]
        public string TranscoderVideoResolutions { get; set; }
        [XmlAttribute(AttributeName = "updatedAt")]
        public string UpdatedAt { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
    }
}
