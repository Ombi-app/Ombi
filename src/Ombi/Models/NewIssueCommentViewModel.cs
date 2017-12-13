namespace Ombi.Models
{
    public class NewIssueCommentViewModel
    {
        public string Comment { get; set; }
        public int? MovieIssueId { get; set; }
        public int? TvIssueId { get; set; }
    }
}