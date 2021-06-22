using System.Collections.Generic;

namespace Ombi.Core.Settings.Models.External
{
    public sealed class TheMovieDbSettings : Ombi.Settings.Settings.Models.Settings
    {
        public bool ShowAdultMovies { get; set; }

        public List<int> ExcludedKeywordIds { get; set; }

        public List<int> ExcludedMovieGenreIds { get; set; }

        public List<int> ExcludedTvGenreIds { get; set; }
    }
}
