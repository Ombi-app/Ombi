using System.ComponentModel.DataAnnotations.Schema;
using Ombi.Helpers;

namespace Ombi.Store.Entities
{
    [Table("NotificationTemplates")]
    public class NotificationTemplates : Entity
    {
        public NotificationType NotificationType { get; set; }
        public NotificationAgent Agent { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool Enabled { get; set; }
    }
}