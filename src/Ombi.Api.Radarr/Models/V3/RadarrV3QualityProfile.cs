namespace Ombi.Api.Radarr.Models.V3
{
    public class RadarrV3QualityProfile
    {
        public string name { get; set; }
        public bool upgradeAllowed { get; set; }
        public int cutoff { get; set; }
        public string preferredTags { get; set; }
        public Item[] items { get; set; }
        public int id { get; set; }
    }

    public class Item
    {
        public Quality quality { get; set; }
        public object[] items { get; set; }
        public bool allowed { get; set; }
    }

    public class Quality
    {
        public int id { get; set; }
        public string name { get; set; }
        public string source { get; set; }
        public int resolution { get; set; }
        public string modifier { get; set; }
    }

}
