namespace Ombi.Api.Radarr.Models
{
    public class RadarrAddOptions
    {
        public bool ignoreEpisodesWithFiles { get; set; }
        public bool ignoreEpisodesWithoutFiles { get; set; }
        public bool searchForMovie { get; set; }
    }
}