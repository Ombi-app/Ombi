using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("RequestSubscription")]
    public class RequestSubscription : Entity
    {
        public string UserId { get; set; }
        public int RequestId { get; set; }
        public RequestType RequestType { get; set; }

        [ForeignKey(nameof(UserId))]
        public OmbiUser User { get; set; }
    }
}