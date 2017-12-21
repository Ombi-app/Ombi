namespace Ombi.Models
{
    public class IssueCountModel
    {
        public int Pending { get; set; }
        public int InProgress { get; set; }
        public int Resolved { get; set; }
    }
}