using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Ombi.Store.Repository.Requests;

namespace Ombi.Store.Entities.Requests
{
    [Table("ChildRequests")]
    public class ChildRequests : BaseRequest
    {
        [ForeignKey(nameof(ParentRequestId))]
        public TvRequests ParentRequest { get; set; }
        public int ParentRequestId { get; set; }
        public int? IssueId { get; set; }
        public SeriesType SeriesType { get; set; }

        /// <summary>
        /// This is to see if the user is subscribed in the UI
        /// </summary>
        [NotMapped]
        public bool Subscribed { get; set; }

        [NotMapped]
        public bool ShowSubscribe { get; set; }

        [NotMapped]
        public DateTime ReleaseYear { get; set; } // Used in the ExistingPlexRequestRule.cs

        [ForeignKey(nameof(IssueId))]
        public List<Issues> Issues { get; set; }

        public List<SeasonRequests> SeasonRequests { get; set; }
    }

    public enum SeriesType
    {
        Standard = 0,
        Anime = 1
    }
}