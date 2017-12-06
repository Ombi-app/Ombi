using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    public class IssueComments : Entity
    {
        public string UserId { get; set; }
        public string Comment { get; set; }
        public int? MovieIssueId { get; set; }
        public int? TvIssueId { get; set; }
        public DateTime Date { get; set; }
        
        [ForeignKey(nameof(MovieIssueId))]
        public MovieIssues MovieIssues { get; set; }
        [ForeignKey(nameof(TvIssueId))]
        public TvIssues TvIssues { get; set; }
        [ForeignKey(nameof(UserId))]
        public OmbiUser User { get; set; }

    }
}