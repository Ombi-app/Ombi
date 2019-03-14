using System;

namespace Ombi.Api.Lidarr.Models
{

    public class CommandResult
    {
        public string name { get; set; }
        public DateTime queued { get; set; }
        public DateTime stateChangeTime { get; set; }
        public bool sendUpdatesToClient { get; set; }
        public string status { get; set; }
        public int id { get; set; }
    }
}
