namespace Ombi.Api.RottenTomatoes.Models
{
    public class RottenTomatoesTvResponse
    {
        public int tvCount { get; set; }
        public TvSeries[] tvSeries { get; set; }
    }

    public class TvSeries
    {
        public string title { get; set; }
        public int startYear { get; set; }
        public int endYear { get; set; }
        public string url { get; set; }
        public string meterClass { get; set; }
        public int meterScore { get; set; }
        public string image { get; set; }
    }

}
