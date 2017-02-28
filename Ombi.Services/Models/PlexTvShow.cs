namespace Ombi.Services.Models
{
    public class PlexTvShow
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ReleaseYear { get; set; }
        public string ProviderId { get; set; }
        public int[] Seasons { get; set; }
        public string Url { get; set; }
        public string ItemId { get; set; }
    }
}
