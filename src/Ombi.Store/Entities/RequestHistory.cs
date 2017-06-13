using System;

namespace Ombi.Store.Entities
{
    public class RequestHistory : Entity
    {
        public int UserId { get; set; }
        public RequestType Type { get; set; }
        public DateTime RequestedDate { get; set; }
        public int RequestId { get; set; }
        
        public virtual RequestBlobs Request { get; set; }
        public virtual User User { get; set; }
    }
}