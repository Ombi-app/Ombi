using Newtonsoft.Json;

namespace Ombi.Api.MusicBrainz.Models
{

    public class ReleaseGroupArt
    {
        public string release { get; set; }
        public Image[] images { get; set; }
    }

    public class Image
    {
        public int edit { get; set; }
        public string id { get; set; }
        public string image { get; set; }
        public Thumbnails thumbnails { get; set; }
        public string comment { get; set; }
        public bool approved { get; set; }
        public bool front { get; set; }
        public string[] types { get; set; }
        public bool back { get; set; }
    }

    public class Thumbnails
    {
        //[JsonProperty("250")]
        //public string px250 { get; set; }
        //[JsonProperty("500")]
        //public string px500 { get; set; }
        //[JsonProperty("1200")]
        //public string px1200 { get; set; }
        public string small { get; set; }
        public string large { get; set; }
    }

}