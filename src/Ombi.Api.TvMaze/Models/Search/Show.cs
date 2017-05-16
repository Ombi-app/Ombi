using System.Collections.Generic;

namespace Ombi.Api.TvMaze.Models
{
    public class Show
    {
        public Links _links { get; set; }
        public Externals externals { get; set; }
        public List<object> genres { get; set; }
        public int id { get; set; }
        public Image image { get; set; }
        public string language { get; set; }
        public string name { get; set; }
        public Network network { get; set; }
        public string premiered { get; set; }
        public Rating rating { get; set; }
        public int? runtime { get; set; }
        public Schedule schedule { get; set; }
        public string status { get; set; }
        public string summary { get; set; }
        public string type { get; set; }
        public int updated { get; set; }
        public string url { get; set; }
        public object webChannel { get; set; }
        public int weight { get; set; }
    }
}