using System.Collections.Generic;

namespace Ombi.Core.Models.Search.V2.Music
{
    public class AlbumInformation
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public string StartYear { get; set; }
        public string EndYear { get; set; }
        public bool IsEnded => EndYear != null;
        public bool Monitored { get; set; }
        public string Type { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string Disambiguation { get; set; }
        public string Cover { get; set; }
        public string Overview { get; set; }
    }
}