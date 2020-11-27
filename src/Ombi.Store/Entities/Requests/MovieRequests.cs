using Ombi.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Ombi.Store.Entities.Requests
{
    [Table("MovieRequests")]
    public class MovieRequests : FullBaseRequest
    {
        public MovieRequests()
        {
            RequestType = RequestType.Movie;
        }
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

        [NotMapped]
        public string RequestStatus {
            get
            {
                if (Available)
                {
                    return "Common.Available";
                }

                if (Denied ?? false)
                {
                    return "Common.Denied";
                }

                if (Approved & !Available)
                {
                    return "Common.ProcessingRequest";
                }

                if (!Approved && !Available)
                {
                    return "Common.PendingApproval";
                }

                return string.Empty;
            }
        }
    }
}
