namespace Ombi.Api.Sonarr.Models.V3
{
    public class LanguageProfiles
    {
        public string name { get; set; }
        public bool upgradeAllowed { get; set; }
        public Cutoff cutoff { get; set; }
        public Languages[] languages { get; set; }
        public int id { get; set; }
    }

    public class Cutoff
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Languages
    {
        public Language languages { get; set; }
        public bool allowed { get; set; }
    }

    public class Language
    {
        public int id { get; set; }
        public string name { get; set; }
    }

}