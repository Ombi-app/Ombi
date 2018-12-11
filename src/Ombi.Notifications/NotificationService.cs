using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Core.Notifications;
using Ombi.Helpers;
using Ombi.Notifications.Models;

namespace Ombi.Notifications
{
    public class NotificationService : INotificationService
    {
        public NotificationService(IServiceProvider provider, ILogger<NotificationService> log)
        {
            Log = log;
            NotificationAgents = new List<INotification>();
            
            var baseSearchType = typeof(BaseNotification<>).Name;

            var ass = typeof(NotificationService).GetTypeInfo().Assembly;

            foreach (var ti in ass.DefinedTypes)
            {
                if (ti?.BaseType?.Name == baseSearchType)
                {
                    var type = ti?.AsType();
                    var ctors = type.GetConstructors();
                    var ctor = ctors.FirstOrDefault();

                    var services = new List<object>();
                    foreach (var param in ctor.GetParameters())
                    {
                        services.Add(provider.GetService(param.ParameterType));
                    }

                    var item = Activator.CreateInstance(type, services.ToArray());
                    NotificationAgents.Add((INotification)item);
                }
            }
        }
        
        private List<INotification> NotificationAgents { get; }
        private ILogger<NotificationService> Log { get; }

        /// <summary>^
        /// Sends a notification to the user. This one is used in normal notification scenarios 
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public async Task Publish(NotificationOptions model)
        {
            var notificationTasks = new List<Task>();

            foreach (var agent in NotificationAgents)
            {
                notificationTasks.Add(NotifyAsync(agent,model));
            }
            await Task.WhenAll(notificationTasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a notification to the user, this is usually for testing the settings.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public async Task Publish(NotificationOptions model, Settings.Settings.Models.Settings settings)
        {
            var notificationTasks = NotificationAgents.Select(notification => NotifyAsync(notification, model, settings));

            await Task.WhenAll(notificationTasks).ConfigureAwait(false);
        }

       
        private async Task NotifyAsync(INotification notification, NotificationOptions model)
        {
            try
            {
                await notification.NotifyAsync(model).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.LogError(LoggingEvents.Notification, ex, "Failed to notify for notification: {@notification}", notification);
            }

        }

        private async Task NotifyAsync(INotification notification, NotificationOptions model, Ombi.Settings.Settings.Models.Settings settings)
        {
            if (model.RequestId == 0)
            {
                throw new ArgumentException("RequestId is not set");
            }
            try
            {
                await notification.NotifyAsync(model, settings).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task PublishTest(NotificationOptions model, Ombi.Settings.Settings.Models.Settings settings, INotification type)
        {
            await type.NotifyAsync(model, settings);
        }
    }
}