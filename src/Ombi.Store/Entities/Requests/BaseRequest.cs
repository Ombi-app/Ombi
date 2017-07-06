using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    public class BaseRequest : Entity
    {
        public string Title { get; set; }
        public bool Approved { get; set; }
        public DateTime RequestedDate { get; set; }
        public bool Available { get; set; }
        public int RequestedUserId { get; set; }
        public bool? Denied { get; set; }
        public string DeniedReason { get; set; }
        public RequestType RequestType { get; set; }

        [ForeignKey(nameof(RequestedUserId))]
        public User RequestedUser { get; set; }


        [NotMapped]
        public bool CanApprove => !Approved && !Available;
    }
}