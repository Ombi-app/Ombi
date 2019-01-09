using Ombi.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Ombi.Store.Entities.Requests
{
    [Table("MovieRequests")]
    public class MovieRequests : FullBaseRequest
    {
        public int TheMovieDbId { get; set; }
        public int? IssueId { get; set; }
        [ForeignKey(nameof(IssueId))]
        public List<Issues> Issues { get; set; }

        [NotMapped]
        public bool Subscribed { get; set; }
        [NotMapped]
        public bool ShowSubscribe { get; set; }

        public int RootPathOverride { get; set; }
        public int QualityOverride { get; set; }

        /// <summary>
        /// Only Use for setting the Language Code, Use the LanguageCode property for reading
        /// </summary>
        public string LangCode { get; set; }

        [NotMapped]
        [JsonIgnore]
        public string LanguageCode => LangCode.IsNullOrEmpty() ? "en" : LangCode;
    }
}
