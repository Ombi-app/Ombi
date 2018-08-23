namespace Ombi.Core.Models.Requests
{
    public class MusicArtistRequestViewModel
    {
        public string ForeignArtistId { get; set; }
        public ArtistRequestOption RequestOption { get; set; }
    }

    public enum ArtistRequestOption
    {
        AllAlbums = 0,
        LatestAlbum = 1,
        FirstAlbum = 2
    }
}