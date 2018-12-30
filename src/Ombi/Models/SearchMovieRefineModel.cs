using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Models
{
    public class SearchMovieRefineModel
    {
        public string SearchTerm { get; set; }
        public int? Year { get; set; }
        public string LanguageCode { get; set; } = "en";
    }
}
