namespace Ombi.Api.Plex.Models
{
    public class Stream
    {
        public int id { get; set; }
        public int streamType { get; set; }
        public bool _default { get; set; }
        public string codec { get; set; }
        public int index { get; set; }
        public int bitrate { get; set; }
        public int bitDepth { get; set; }
        public string chromaSubsampling { get; set; }
        public float frameRate { get; set; }
        public bool hasScalingMatrix { get; set; }
        public int height { get; set; }
        public string level { get; set; }
        public string profile { get; set; }
        public int refFrames { get; set; }
        public string scanType { get; set; }
        public int width { get; set; }
        public int channels { get; set; }
        public string language { get; set; }
        public string languageCode { get; set; }
        public string audioChannelLayout { get; set; }
        public int samplingRate { get; set; }
        public bool selected { get; set; }
    }
}