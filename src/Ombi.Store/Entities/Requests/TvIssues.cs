using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    [Table("TvIssues")]
    public class TvIssues : IssuesBase
    {
        public int TvId { get; set; }
        [ForeignKey(nameof(TvId))]
        public ChildRequests Child { get; set; }
    }
}