namespace Ombi.Core.Models.Requests
{
    public class MusicAlbumRequestViewModel
    {
        public string ForeignAlbumId { get; set; }
        public string RequestedByAlias { get; set; }
    }

    public class MusicArtistRequestViewModel
    {
        public string ForeignArtistId { get; set; }
        public bool Monitored { get; set; }
        public string RequestedByAlias { get; set; }
        public string Monitor { get; set; }
        public bool SearchForMissingAlbums { get; set; }
    }
}