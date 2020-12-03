using System.Collections.Generic;

namespace Ombi.Api.Radarr.Models
{
    public class RadarrAddMovieResponse
    {

        public RadarrAddMovieResponse()
        {
            images = new List<Image>();
        }
        public RadarrError Error { get; set; }
        public RadarrAddOptions addOptions { get; set; }
        public string title { get; set; }
        public string rootFolderPath { get; set; }
        public int qualityProfileId { get; set; }
        public bool monitored { get; set; }
        public int tmdbId { get; set; }
        public List<Image> images { get; set; }
        public string titleSlug { get; set; }
        public int year { get; set; }
        public string minimumAvailability { get; set; }
    }
}