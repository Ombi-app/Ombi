namespace Ombi.Api.Plex.Models
{
    public class Medium
    {
        public string videoResolution { get; set; }
        public int id { get; set; }
        public int duration { get; set; }
        public int bitrate { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public float aspectRatio { get; set; }
        public int audioChannels { get; set; }
        public string audioCodec { get; set; }
        public string videoCodec { get; set; }
        public string container { get; set; }
        public string videoFrameRate { get; set; }
        public string audioProfile { get; set; }
        public string videoProfile { get; set; }
        //public Part[] Part { get; set; }
    }
}