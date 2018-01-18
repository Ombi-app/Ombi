using Ombi.Store.Entities.Requests;

namespace Ombi.Models
{
    public class IssueStateViewModel
    {
        public int IssueId { get; set; }
        public IssueStatus Status { get; set; }
    }
}