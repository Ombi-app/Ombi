using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Ombi.Helpers;

namespace Ombi.Store.Entities
{
    [Table(nameof(UserNotificationPreferences))]
    public class UserNotificationPreferences : Entity
    {
        public string UserId { get; set; }
        public NotificationAgent Agent { get; set; }
        public bool Enabled { get; set; }
        public string Value { get; set; }

        [ForeignKey(nameof(UserId))]
        public OmbiUser User { get; set; }
    }
}
