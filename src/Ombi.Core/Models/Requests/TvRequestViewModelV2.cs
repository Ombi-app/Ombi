using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ombi.Core.Models.Requests
{
    public class TvRequestViewModelV2 : TvRequestViewModelBase
    {
        public int TheMovieDbId { get; set; }
        public string languageCode { get; set; } = "en";
    }
}