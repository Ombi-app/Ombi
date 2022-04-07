using Newtonsoft.Json;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Models.Requests
{
    public class MovieRequestViewModel : BaseRequestOptions
    {
        public int TheMovieDbId { get; set; }
        public string LanguageCode { get; set; } = "en";

        public bool Is4kRequest { get; set; }

        /// <summary>
        /// This is only set from a HTTP Header
        /// </summary>
        [JsonIgnore]
        public string RequestedByAlias { get; set; }

        /// <summary>
        /// Only set via list imports
        /// </summary>
        [JsonIgnore]
        public RequestSource Source { get; set; } = RequestSource.Ombi;
    }
}