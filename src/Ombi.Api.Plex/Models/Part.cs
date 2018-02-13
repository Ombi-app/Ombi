namespace Ombi.Api.Plex.Models
{
    public class Part
    {
        public int id { get; set; }
        public string key { get; set; }
        public string duration { get; set; }
        public string file { get; set; }
        public string size { get; set; }
        public string audioProfile { get; set; }
        public string container { get; set; }
        public string videoProfile { get; set; }
        public Stream[] Stream { get; set; }
    }
}