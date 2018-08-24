using Ombi.Api.Lidarr.Models;

namespace Ombi.Core.Models.Search
{
    public class SearchArtistViewModel
    {
        public string ArtistName { get; set; }
        public string ForignArtistId { get; set; }
        public string Overview { get; set; }
        public string Disambiguation { get; set; }
        public string Banner { get; set; }
        public string Poster { get; set; }
        public string Logo { get; set; }
        public bool Monitored { get; set; }
        public string ArtistType { get; set; }
        public string CleanName { get; set; }
        public Link[] Links { get; set; } // Couldn't be bothered to map it
    }
}