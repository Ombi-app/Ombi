using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    public class MobileDevices : Entity
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public DateTime AddedAt { get; set; }
        [ForeignKey(nameof(UserId))]
        public OmbiUser User { get; set; }
    }
}