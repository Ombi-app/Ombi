using System.Collections.Generic;

namespace Ombi.Core.Models.Search.V2.Music
{
    public class ArtistInformation
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string StartYear { get; set; }
        public string EndYear { get; set; }
        public string Type { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string Disambiguation { get; set; }
        public List<ReleaseGroup> ReleaseGroups { get; set; }
        public List<ArtistLinks> Links { get; set; } 
    }

    public class ArtistLinks
    {
        public string Image { get; set; }
        public string Imdb { get; set; }
        public string LastFm { get; set; }
        public string Discogs { get; set; }
        public string BandsInTown { get; set; }
        public string Website { get; set; }
        public string YouTube { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
    }

    public class ReleaseGroup
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ReleaseDate { get; set; }
        public string Type { get; set; }

    }
}