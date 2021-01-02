namespace Ombi.Api.Jellyfin.Models.Movie
{
    public class JellyfinMediasource
    {
        public string Protocol { get; set; }
        public string Id { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public string Container { get; set; }
        public string Name { get; set; }
        public bool IsRemote { get; set; }
        public string ETag { get; set; }
        public long RunTimeTicks { get; set; }
        public bool ReadAtNativeFramerate { get; set; }
        public bool SupportsTranscoding { get; set; }
        public bool SupportsDirectStream { get; set; }
        public bool SupportsDirectPlay { get; set; }
        public bool IsInfiniteStream { get; set; }
        public bool RequiresOpening { get; set; }
        public bool RequiresClosing { get; set; }
        public bool SupportsProbing { get; set; }
        public string VideoType { get; set; }
        public JellyfinMediastream[] MediaStreams { get; set; }
        public object[] PlayableStreamFileNames { get; set; }
        public object[] Formats { get; set; }
        public int Bitrate { get; set; }
        public int DefaultAudioStreamIndex { get; set; }

    }
}