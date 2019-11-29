using System.Collections.Generic;

namespace Ombi.Notifications
{
    public class NotificationMessageContent
    {
        public bool Disabled { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Image { get; set; }
        public IReadOnlyDictionary<string, string> Data { get; set; }
    }
}