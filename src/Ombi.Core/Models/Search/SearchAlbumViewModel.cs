using System;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.Search
{
    public class SearchAlbumViewModel : SearchViewModel
    {
        public string Title { get; set; }
        public string ForeignAlbumId { get; set; }
        public bool Monitored { get; set; }
        public string AlbumType { get; set; }
        public decimal Rating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string ArtistName { get; set; }
        public string ForeignArtistId { get; set; }
        public string Cover { get; set; }
        public string Disk { get; set; }
        public decimal PercentOfTracks { get; set; }
        public override RequestType Type => RequestType.Album;
        public bool PartiallyAvailable => PercentOfTracks != 100 && PercentOfTracks > 0;
        public bool FullyAvailable => PercentOfTracks == 100;
    }
}