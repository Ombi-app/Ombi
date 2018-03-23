
using System.Collections.Generic;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.UI
{
    /// <summary>
    /// The view model for the notification settings page
    /// </summary>
    /// <seealso cref="NewsletterNotificationViewModel" />
    public class NewsletterNotificationViewModel : NewsletterSettings
    {
        /// <summary>
        /// Gets or sets the notification templates.
        /// </summary>
        /// <value>
        /// The notification templates.
        /// </value>
        public NotificationTemplates NotificationTemplate { get; set; }
        
    }
}
