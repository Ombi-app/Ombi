using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    [Table("Issues")]
    public class Issues : Entity
    {
        public string Title { get; set; }
        public RequestType RequestType { get; set; }
        public string ProviderId { get; set; }
        public int? RequestId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        //public int SeasonNumber { get; set; }
        //public int EpisodeNumber { get; set; }
        public int IssueCategoryId { get; set; }
        [ForeignKey(nameof(IssueCategoryId))]
        public IssueCategory IssueCategory { get; set; }
        public IssueStatus Status { get; set; }
        public DateTime? ResovledDate { get; set; }
        public DateTime CreatedDate { get; set; }
        [ForeignKey(nameof(UserReported))]
        public string UserReportedId { get; set; }
        public OmbiUser UserReported { get; set; }
        public List<IssueComments> Comments { get; set; }
    }

    public enum IssueStatus
    {
        Pending = 0,
        InProgress = 1,
        Resolved = 2,
        Deleted = 3,
    }
}