using System.Collections.Generic;
using Newtonsoft.Json;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Models.Requests
{
    public class TvRequestViewModelV2 : TvRequestViewModelBase
    {
        public int TheMovieDbId { get; set; }
        public string languageCode { get; set; } = "en";
        public RequestSource Source { get; set; } = RequestSource.Ombi;
    }
}