using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
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
        [JsonIgnore]
        public OmbiUser User { get; set; }
    }
}
