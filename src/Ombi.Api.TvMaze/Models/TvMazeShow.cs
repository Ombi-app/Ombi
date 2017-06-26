using System.Collections.Generic;

namespace Ombi.Api.TvMaze.Models
{
    public class TvMazeShow
    {

        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string language { get; set; }
        public List<string> genres { get; set; }
        public string status { get; set; }
        public double runtime { get; set; }
        public string premiered { get; set; }
        public Schedule schedule { get; set; }
        public Rating rating { get; set; }
        public int weight { get; set; }
        public Network network { get; set; }
        public object webChannel { get; set; }
        public Externals externals { get; set; }
        public Image image { get; set; }
        public string summary { get; set; }
        public int updated { get; set; }
        public Links _links { get; set; }
        public Embedded _embedded { get; set; }
    }

    public class TvMazeCustomSeason
    {
        public int SeasonNumber { get; set; }
        public List<int> EpisodeNumber { get; set; }
    }

    public class Season
    {
        public int id { get; set; }
        public string url { get; set; }
        public int number { get; set; }
        public string name { get; set; }
        public int? episodeOrder { get; set; }
        public string premiereDate { get; set; }
        public string endDate { get; set; }
        public Network2 network { get; set; }
        public object webChannel { get; set; }
        public Image2 image { get; set; }
        public string summary { get; set; }
        public Links2 _links { get; set; }
    }
    public class Country2
    {
        public string name { get; set; }
        public string code { get; set; }
        public string timezone { get; set; }
    }

    public class Network2
    {
        public int id { get; set; }
        public string name { get; set; }
        public Country2 country { get; set; }
    }

    public class Image2
    {
        public string medium { get; set; }
        public string original { get; set; }
    }

    public class Self2
    {
        public string href { get; set; }
    }

    public class Links2
    {
        public Self2 self { get; set; }
    }

    public class Embedded
    {
        public List<Season> seasons { get; set; }
    }
}