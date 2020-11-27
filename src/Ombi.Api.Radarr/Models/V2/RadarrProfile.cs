using System.Collections.Generic;

namespace Ombi.Api.Radarr.Models
{
    public class RadarrProfile
    {
        public string name { get; set; }
        public Cutoff cutoff { get; set; }
        public List<Item> items { get; set; }
        public int id { get; set; }
    }
}
