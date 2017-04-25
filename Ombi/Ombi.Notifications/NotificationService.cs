using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Settings.Models;
using Ombi.Notifications.Models;

namespace Ombi.Notifications
{
    public class NotificationService : INotificationService
    {
        public ConcurrentDictionary<string, INotification> Observers { get; } = new ConcurrentDictionary<string, INotification>();

        /// <summary>
        /// Sends a notification to the user. This one is used in normal notification scenarios 
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public async Task Publish(NotificationModel model)
        {
            var notificationTasks = Observers.Values.Select(notification => NotifyAsync(notification, model));

            await Task.WhenAll(notificationTasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a notification to the user, this is usually for testing the settings.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public async Task Publish(NotificationModel model, Settings.Settings.Models.Settings settings)
        {
            var notificationTasks = Observers.Values.Select(notification => NotifyAsync(notification, model, settings));

            await Task.WhenAll(notificationTasks).ConfigureAwait(false);
        }

        public void Subscribe(INotification notification)
        {
            Observers.TryAdd(notification.NotificationName, notification);
        }

        public void UnSubscribe(INotification notification)
        {
            Observers.TryRemove(notification.NotificationName, out notification);
        }

        private async Task NotifyAsync(INotification notification, NotificationModel model)
        {
            try
            {
                await notification.NotifyAsync(model).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }

        }

        private async Task NotifyAsync(INotification notification, NotificationModel model, Settings.Settings.Models.Settings settings)
        {
            try
            {
                await notification.NotifyAsync(model, settings).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task PublishTest(NotificationModel model, Settings.Settings.Models.Settings settings, INotification type)
        {
            await type.NotifyAsync(model, settings);
        }
    }
}