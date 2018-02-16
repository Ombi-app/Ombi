using System;

namespace Ombi.Api.Radarr
{
    public partial class RadarrApi
    {
        public class CommandResult
        {
            public string name { get; set; }
            public DateTime startedOn { get; set; }
            public DateTime stateChangeTime { get; set; }
            public bool sendUpdatesToClient { get; set; }
            public string state { get; set; }
            public int id { get; set; }
        }
    }
}
