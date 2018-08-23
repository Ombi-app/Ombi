namespace Ombi.Store.Entities.Requests
{
    public class ArtistRequest : BaseRequest
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
    }
}