using Ombi.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System;

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

        /// <summary>
        /// This is only used during the request process to identify if
        /// it's a regular request or a 4k
        /// </summary>
        [NotMapped]
        public bool Is4kRequest { get; set; }

        public int RootPathOverride { get; set; }
        public int QualityOverride { get; set; }

        public bool Has4KRequest  { get; set; }

        public bool Approved4K { get; set; }
        public DateTime MarkedAsApproved4K { get; set; }
        public DateTime RequestedDate4k { get; set; }
        public bool Available4K { get; set; }
        public DateTime? MarkedAsAvailable4K { get; set; }
        public bool? Denied4K { get; set; }
        public DateTime MarkedAsDenied4K { get; set; }
        public string DeniedReason4K { get; set; }

        [NotMapped]
        public RequestCombination RequestCombination
        {
            get
            {
                if (Has4KRequest && RequestedDate != default)
                {
                    return RequestCombination.Both;
                }
                if (Has4KRequest) { return RequestCombination.FourK; }

                return RequestCombination.Normal;
            }
        }


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
                // A request tracks a standard copy and a 4K copy independently, so report
                // whichever one is still outstanding. The standard copy takes precedence
                // while both are, which matches how the media details page reads.
                var hasStandardRequest = RequestCombination != RequestCombination.FourK;

                if (hasStandardRequest && !Available)
                {
                    if (Denied ?? false)
                    {
                        return "Common.Denied";
                    }

                    return Approved ? "Common.ProcessingRequest" : "Common.PendingApproval";
                }

                if (Has4KRequest && !Available4K)
                {
                    if (Denied4K ?? false)
                    {
                        return "Common.RequestDenied4K";
                    }

                    return Approved4K ? "Common.ProcessingRequest4K" : "Common.PendingApproval4K";
                }

                if (hasStandardRequest && Available)
                {
                    return "Common.Available";
                }

                if (Has4KRequest && Available4K)
                {
                    return "Common.Available4K";
                }

                return string.Empty;
            }
        }

        [NotMapped]
        public override bool CanApprove => !Approved && !Available || !Approved4K && !Available4K;
        
        [NotMapped]
        public bool WatchedByRequestedUser { get; set; }
        [NotMapped]
        public int PlayedByUsersCount { get; set; }
    }
}
