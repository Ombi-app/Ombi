using System;
using System.Collections.Generic;
using Ombi.Helpers;
using Ombi.Store;
using Ombi.Store.Entities;

namespace Ombi.Notifications.Models
{
    public class NotificationOptions
    {
        public int RequestId { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public NotificationType NotificationType { get; set; }
        public RequestType RequestType { get; set; }
        public string Recipient { get; set; }
        public string AdditionalInformation { get; set; }
        public string UserId { get; set; }

        public Dictionary<string,string> Substitutes { get; set; } = new Dictionary<string, string>();
        
    }
}