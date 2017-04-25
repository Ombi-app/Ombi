using System;
using Ombi.Store.Entities;

namespace Ombi.Notifications.Models
{
    public class NotificationModel
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime DateTime { get; set; }
        public NotificationType NotificationType { get; set; }
        public string User { get; set; }
        public string UserEmail { get; set; }
        public RequestType RequestType { get; set; }
        public string ImgSrc { get; set; }
    }
}