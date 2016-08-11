namespace PlexRequests.Services.Models
{
    public class PlexTvShow
    {
        public string Title { get; set; }
        public string ReleaseYear { get; set; }
        public string ProviderId { get; set; }
        public int[] Seasons { get; set; }
    }
}
