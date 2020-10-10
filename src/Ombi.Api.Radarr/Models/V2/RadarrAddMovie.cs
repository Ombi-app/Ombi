using System.Collections.Generic;

namespace Ombi.Api.Radarr.Models
{
    public class RadarrAddMovieResponse : RadarrAddMovie
    {
        public RadarrAddMovieResponse()
        {
            images = new List<string>();
        }
        public List<string> images { get; set; }
    }


    public class RadarrAddMovie
    {

        public RadarrAddMovie()
        {
        }
        public RadarrError Error { get; set; }
        public RadarrAddOptions addOptions { get; set; }
        public string title { get; set; }
        public string rootFolderPath { get; set; }
        public int qualityProfileId { get; set; }
        public bool monitored { get; set; }
        public int tmdbId { get; set; }
        public string titleSlug { get; set; }
        public int year { get; set; }
        public string minimumAvailability { get; set; }
    }
}