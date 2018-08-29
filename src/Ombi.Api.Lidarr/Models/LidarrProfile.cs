using System.Collections.Generic;

namespace Ombi.Api.Lidarr.Models
{
    public class Quality
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Item
    {
        public Quality quality { get; set; }
        public bool allowed { get; set; }
    }

    public class LidarrProfile
{
        public string name { get; set; }
        public List<Item> items { get; set; }
        public int id { get; set; }
    }
}