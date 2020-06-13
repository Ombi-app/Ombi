using System.Collections.Generic;

namespace Ombi.Notifications.Models
{
    public class NotificationMessage
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public string To { get; set; }

        public Dictionary<string, string> Other { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    }
}