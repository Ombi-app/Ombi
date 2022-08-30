using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Core.Notifications;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Quartz;

namespace Ombi.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IServiceProvider _provider;

        public NotificationService(IServiceProvider provider, ILogger<NotificationService> log)
        {
            _provider = provider;
            Log = log;
            NotificationAgents = new List<INotification>();
            PopulateAgents();
        }

        protected List<INotification> NotificationAgents { get; }
        private ILogger<NotificationService> Log { get; }

        /// <summary>
        /// Sends a notification to the user. This one is used in normal notification scenarios 
        /// </summary>
        /// <param name="context">The model.</param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.MergedJobDataMap;
            var model = (NotificationOptions)dataMap.Get(JobDataKeys.NotificationOptions);
            
            foreach (var agent in NotificationAgents)
            {
                await NotifyAsync(agent, model);
            }
        }

        private async Task NotifyAsync(INotification notification, NotificationOptions model)
        {
            try
            {
                await notification.NotifyAsync(model);
            }
            catch (Exception ex)
            {
                Log.LogError(LoggingEvents.Notification, ex, "Failed to notify for notification: {@notification}", notification);
            }

        }
        
        protected void PopulateAgents()
        {
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
                    foreach (var param in ctor?.GetParameters())
                    {
                        services.Add(_provider.GetService(param.ParameterType));
                    }

                    var item = Activator.CreateInstance(type, services.ToArray());
                    NotificationAgents.Add((INotification)item);
                }
            }
        }
    }
}