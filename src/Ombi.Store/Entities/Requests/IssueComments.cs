using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    public class IssueComments : Entity
    {
        public string UserId { get; set; }
        public string Comment { get; set; }
        public int? IssuesId { get; set; }
        public DateTime Date { get; set; }
        
        [ForeignKey(nameof(IssuesId))]
        public Issues Issues{ get; set; }
        [ForeignKey(nameof(UserId))]
        public OmbiUser User { get; set; }
    }
}