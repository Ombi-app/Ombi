using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("Votes")]
    public class Votes : Entity
    {
        public int RequestId { get; set; }
        public VoteType VoteType { get; set; }
        public RequestType RequestType { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public bool Deleted { get; set; }

        [ForeignKey(nameof(UserId))]
        public OmbiUser User { get; set; }
    }

    public enum VoteType
    {
        Upvote = 0,
        Downvote = 1
    }
}