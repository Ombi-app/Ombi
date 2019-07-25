using System.Collections.Generic;

namespace Ombi.Core.Models.Search.V2.Music
{
    public class ArtistInformation
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string StartYear { get; set; }
        public string EndYear { get; set; }
        public bool IsEnded => EndYear != null;
        public string Type { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string Disambiguation { get; set; }
        public List<ReleaseGroup> ReleaseGroups { get; set; }
        public ArtistLinks Links { get; set; }
        public List<BandMember> Members { get; set; }
    }

    public class BandMember
    {
        public string Name { get; set; }
        public string[] Attributes { get; set; }
        public bool IsCurrentMember { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }

    public class ArtistLinks
    {
        public string Image { get; set; }
        public string Imdb { get; set; }
        public string LastFm { get; set; }
        public string Discogs { get; set; }
        public string AllMusic { get; set; }
        public string HomePage { get; set; }
        public string YouTube { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string BbcMusic { get; set; }
        public string MySpace { get; set; }
        public string OnlineCommunity { get; set; }
        public string Spotify { get; set; }
        public string Instagram { get; set; }
        public string Vk { get; set; }
        public string Deezer { get; set; }
        public string Google { get; set; }
        public string Apple { get; set; }
    }

    public class ReleaseGroup
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ReleaseDate { get; set; }
        public string Type { get; set; }

    }
}