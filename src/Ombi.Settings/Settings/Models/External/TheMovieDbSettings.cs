namespace Ombi.Core.Settings.Models.External
{
    public sealed class TheMovieDbSettings : Ombi.Settings.Settings.Models.Settings
    {
        public bool ShowAdultMovies { get; set; }

        public string ExcludedKeywordIds { get; set; }
    }
}
