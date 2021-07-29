using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Api.TheMovieDb.Models
{
    public class WatchProvidersResults
    {
        public int provider_id { get; set; }
        public string logo_path { get; set; }
        public string provider_name { get; set; }
        public string origin_country { get; set; }
    }
}
