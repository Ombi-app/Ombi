namespace Ombi.Settings.Settings.Models.External
{
    public class DogNzbSettings : Settings
    {
        public bool Enabled { get; set; }
        public string ApiKey { get; set; }
        public bool Movies { get; set; }
        public bool TvShows { get; set; }
    }
}