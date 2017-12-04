using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    public class IssuesBase : Entity
    {
        public string Subect { get; set; }
        public string Description { get; set; }
        public int IssueCategoryId { get; set; }
        [ForeignKey(nameof(IssueCategoryId))]
        public IssueCategory IssueCategory { get; set; }
    }
}