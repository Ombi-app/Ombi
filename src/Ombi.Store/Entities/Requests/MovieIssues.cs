using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    [Table("MovieIssues")]
    public class MovieIssues : IssuesBase
    {
        public int MovieId { get; set; }
        [ForeignKey(nameof(MovieId))]
        public MovieRequests Movie { get; set; }

        public List<IssueComments> Comments { get; set; }
    }
}