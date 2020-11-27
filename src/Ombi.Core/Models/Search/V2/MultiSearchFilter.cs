using System;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Core.Models.Search.V2
{
    public class MultiSearchFilter
    {
        public bool Movies { get; set; }
        public bool TvShows { get; set; }
        public bool Music { get; set; }
        public bool People { get; set; }
    }
}
