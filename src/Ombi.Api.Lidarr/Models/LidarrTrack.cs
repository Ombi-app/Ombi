using System;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Api.Lidarr.Models
{
    public class LidarrTrack
    {
        public int artistId { get; set; }
        public int trackFileId { get; set; }
        public int albumId { get; set; }
        public bool _explicit { get; set; }
        public int absoluteTrackNumber { get; set; }
        public string trackNumber { get; set; }
        public string title { get; set; }
        public int duration { get; set; }
        public int mediumNumber { get; set; }
        public bool hasFile { get; set; }
        public bool monitored { get; set; }
        public int id { get; set; }
    }
}
