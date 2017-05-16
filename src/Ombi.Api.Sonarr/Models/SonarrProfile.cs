using System.Collections.Generic;

namespace Ombi.Api.Sonarr.Models
{
    public class SonarrProfile
    {
        public string name { get; set; }
        public Cutoff cutoff { get; set; }
        public List<Item> items { get; set; }
        public int id { get; set; }
    }
}