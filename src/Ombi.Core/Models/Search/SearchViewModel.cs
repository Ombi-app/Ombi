namespace Ombi.Core.Models.Search
{
    public class SearchViewModel
    {
        public int Id { get; set; }
        public bool Approved { get; set; }
        public bool Requested { get; set; }
        public bool Available { get; set; }
        public string PlexUrl { get; set; }
    }
}