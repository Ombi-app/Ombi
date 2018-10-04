using Ombi.Store.Entities;

namespace Ombi.Core.Models.UI
{
    public class VoteViewModel
    {
        public int RequestId { get; set; }
        public RequestType RequestType { get; set; }
        public string Image { get; set; }
        public string Background { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool AlreadyVoted { get; set; }
        public VoteType MyVote { get; set; }
    }
}