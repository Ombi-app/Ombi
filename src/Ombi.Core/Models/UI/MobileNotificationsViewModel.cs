using System.Collections.Generic;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.UI
{
    public class MobileNotificationsViewModel : MobileNotificationSettings
    {
        /// <summary>
        /// Gets or sets the notification templates.
        /// </summary>
        /// <value>
        /// The notification templates.
        /// </value>
        public List<NotificationTemplates> NotificationTemplates { get; set; }
    }
}
