using Ombi.Store.Entities;

namespace Ombi.Core.Models.Search.V2.Music
{
    public class ReleaseGroup : SearchViewModel
    {
        public new string Id { get; set; }
        public override RequestType Type => RequestType.Album;
        public string Title { get; set; }
        public string ReleaseDate { get; set; }
        public string ReleaseType { get; set; }
        public decimal PercentOfTracks { get; set; }
        public bool Monitored { get; set; }
        public bool PartiallyAvailable => PercentOfTracks != 100 && PercentOfTracks > 0;
        public bool FullyAvailable => PercentOfTracks == 100;
    }
}