using System.Collections.Generic;

namespace Ombi.Api.TvMaze.Models
{
    public class Schedule
    {
        public List<object> days { get; set; }
        public string time { get; set; }
    }
}