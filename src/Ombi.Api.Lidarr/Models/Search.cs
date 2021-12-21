using System;
using System.Net.Mime;

namespace Ombi.Api.Lidarr.Models
{
    public class Search
    {
        public ArtistLookup artist { get; set; }
        public AlbumLookup album { get; set; }
        public string foreignId { get; set; }
        public string id { get; set; }
    }
}