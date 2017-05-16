namespace Ombi.Api.Plex.Models.Status
{
    public class Mediacontainer
    {
        public int size { get; set; }
        public bool allowCameraUpload { get; set; }
        public bool allowChannelAccess { get; set; }
        public bool allowMediaDeletion { get; set; }
        public bool allowSharing { get; set; }
        public bool allowSync { get; set; }
        public bool backgroundProcessing { get; set; }
        public bool certificate { get; set; }
        public bool companionProxy { get; set; }
        public string countryCode { get; set; }
        public string diagnostics { get; set; }
        public bool eventStream { get; set; }
        public string friendlyName { get; set; }
        public bool hubSearch { get; set; }
        public bool itemClusters { get; set; }
        public string machineIdentifier { get; set; }
        public bool mediaProviders { get; set; }
        public bool multiuser { get; set; }
        public bool myPlex { get; set; }
        public string myPlexMappingState { get; set; }
        public string myPlexSigninState { get; set; }
        public bool myPlexSubscription { get; set; }
        public string myPlexUsername { get; set; }
        public bool photoAutoTag { get; set; }
        public string platform { get; set; }
        public string platformVersion { get; set; }
        public bool pluginHost { get; set; }
        public bool readOnlyLibraries { get; set; }
        public bool requestParametersInCookie { get; set; }
        public int streamingBrainVersion { get; set; }
        public bool sync { get; set; }
        public int transcoderActiveVideoSessions { get; set; }
        public bool transcoderAudio { get; set; }
        public bool transcoderLyrics { get; set; }
        public bool transcoderPhoto { get; set; }
        public bool transcoderSubtitles { get; set; }
        public bool transcoderVideo { get; set; }
        public string transcoderVideoBitrates { get; set; }
        public string transcoderVideoQualities { get; set; }
        public string transcoderVideoResolutions { get; set; }
        public int updatedAt { get; set; }
        public bool updater { get; set; }
        public string version { get; set; }
        public bool voiceSearch { get; set; }
        public Directory[] Directory { get; set; }
    }
}