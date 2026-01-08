using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Api.External.ExternalApis.TheMovieDb.Models
{
    public class DiscoverModel
    {
        public string Type { get; set; }
        public int? ReleaseYear { get; set; }
        public int? Decade { get; set; }
        public List<int> GenreIds { get; set; } = new List<int>();
        public List<int> KeywordIds { get; set; } = new List<int>();
        public List<int> WatchProviders { get; set; } = new List<int>();
        public List<int> Companies { get; set; } = new List<int>();
    }
}
