using System;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Api.Sonarr.Models
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
