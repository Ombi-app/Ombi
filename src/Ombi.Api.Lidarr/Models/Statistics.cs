namespace Ombi.Api.Lidarr.Models
{
    public class Statistics
    {
        public int albumCount { get; set; }
        public int trackFileCount { get; set; }
        public int trackCount { get; set; }
        public int totalTrackCount { get; set; }
        public long sizeOnDisk { get; set; }
        public decimal percentOfEpisodes { get; set; }
    }
}