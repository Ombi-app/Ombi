namespace Ombi.Api.FanartTv.Models
{
    public class TvResult
    {
        public string name { get; set; }
        public string thetvdb_id { get; set; }
        public Content[] hdtvlogo { get; set; }
        public Content[] seasonposter { get; set; }
        public Content[] seasonthumb { get; set; }
        public Content[] characterart { get; set; }
        public Content[] clearlogo { get; set; }
        public Content[] hdclearart { get; set; }
        public Content[] tvposter { get; set; }
        public Content[] showbackground { get; set; }
        public Content[] tvthumb { get; set; }
        public Content[] clearart { get; set; }
        public Content[] tvbanner { get; set; }
        public Content[] seasonbanner { get; set; }
    }

    public class Content
    {
        public string id { get; set; }
        public string url { get; set; }
        public string lang { get; set; }
        public string likes { get; set; }
    }
}
