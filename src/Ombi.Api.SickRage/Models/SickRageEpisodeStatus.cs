namespace Ombi.Api.SickRage.Models
{
    public class SickRageEpisodeStatus
    {
        public Data[] data { get; set; }
        public string message { get; set; }
        public string result { get; set; }
    }

    public class SickRageEpisodeSetStatus
    {
        public Data[] data { get; set; }
        public string message { get; set; }
        public string result { get; set; }
    }

    public class Data
    {
        public int episode { get; set; }
        public string message { get; set; }
        public string result { get; set; }
        public int season { get; set; }
        public string status { get; set; }
    }
}