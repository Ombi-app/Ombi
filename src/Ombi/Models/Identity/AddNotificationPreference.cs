using Ombi.Helpers;

namespace Ombi.Models.Identity
{
    public class AddNotificationPreference
    {
        public NotificationAgent Agent { get; set; }
        public string UserId { get; set; }
        public string Value { get; set; }
        public bool Enabled { get; set; }
    }
}