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


        [ForeignKey(nameof(IssueId))]
        public List<Issues> Issues { get; set; }

        public List<SeasonRequests> SeasonRequests { get; set; }
    }
}