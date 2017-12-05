using System;
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

        public IssueStatus Status { get; set; }
        public string AdminComment { get; set; }
        public DateTime? ResovledDate { get; set; }
    }

    public enum IssueStatus
    {
        Pending = 0,
        InProgress = 1,
        Resolved = 2,
    }
}