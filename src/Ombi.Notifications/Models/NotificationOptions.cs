using System;
using Ombi.Helpers;
using Ombi.Store;
using Ombi.Store.Entities;

namespace Ombi.Notifications.Models
{
    public class NotificationOptions
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public NotificationType NotificationType { get; set; }
        public string RequestedUser { get; set; }
        public string UserEmail { get; set; }
        public RequestType RequestType { get; set; }
        public string ImgSrc { get; set; }
    }
}