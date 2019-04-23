using System;

namespace Ombi.Api.TvMaze.Models.V2
{
    public class FullSearch
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string language { get; set; }
        public string[] genres { get; set; }
        public string status { get; set; }
        public int runtime { get; set; }
        public string premiered { get; set; }
        public string officialSite { get; set; }
        public Schedule schedule { get; set; }
        public Rating rating { get; set; }
        public int weight { get; set; }
        public Network network { get; set; }
        public object webChannel { get; set; }
        public Externals externals { get; set; }
        public Image image { get; set; }
        public string summary { get; set; }
        public int updated { get; set; }
        public _Links _links { get; set; }
        public _Embedded _embedded { get; set; }
    }

    public class Schedule
    {
        public string time { get; set; }
        public string[] days { get; set; }
    }

    public class Rating
    {
        public float average { get; set; }
    }

    public class Network
    {
        public int id { get; set; }
        public string name { get; set; }
        public Country country { get; set; }
    }

    public class Country
    {
        public string name { get; set; }
        public string code { get; set; }
        public string timezone { get; set; }
    }

    public class Externals
    {
        public int tvrage { get; set; }
        public int thetvdb { get; set; }
        public string imdb { get; set; }
    }

    public class Image
    {
        public string medium { get; set; }
        public string original { get; set; }
    }

    public class _Links
    {
        public Self self { get; set; }
        public Previousepisode previousepisode { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class Previousepisode
    {
        public string href { get; set; }
    }

    public class _Embedded
    {
        public Cast[] cast { get; set; }
        public Crew[] crew { get; set; }
        public Episode[] episodes { get; set; }
    }

    public class Cast
    {
        public Person person { get; set; }
        public Character character { get; set; }
        public bool self { get; set; }
        public bool voice { get; set; }
    }

    public class Person
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public Country country { get; set; }
        public string birthday { get; set; }
        public object deathday { get; set; }
        public string gender { get; set; }
        public Image image { get; set; }
        public _Links _links { get; set; }
    }
    

    public class Character
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public Image image { get; set; }
        public _Links _links { get; set; }
    }

    public class Crew
    {
        public string type { get; set; }
        public Person person { get; set; }
    }
    
    public class Episode
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public int season { get; set; }
        public int number { get; set; }
        public string airdate { get; set; }
        public string airtime { get; set; }
        public DateTime airstamp { get; set; }
        public int runtime { get; set; }
        public Image image { get; set; }
        public string summary { get; set; }
        public _Links _links { get; set; }
    }

}
