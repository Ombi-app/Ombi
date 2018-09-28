namespace Ombi.Api.Lidarr.Models
{
    public class AlbumByArtistResponse
    {
        public Album[] Albums { get; set; }
        public string ArtistName { get; set; }
        public string Disambiguation { get; set; }
        public string Id { get; set; }
        public Image[] Images { get; set; }
        public Link[] Links { get; set; }
        public string Overview { get; set; }
        public Rating Rating { get; set; }
        public string SortName { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
    }

    public class Rating
    {
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    public class Album
    {
        public string Disambiguation { get; set; }
        public string Id { get; set; }
        public string ReleaseDate { get; set; }
        public string[] ReleaseStatuses { get; set; }
        public string[] SecondaryTypes { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
    }
}