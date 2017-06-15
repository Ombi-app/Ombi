using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Store;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Notifications
{
    public abstract class BaseNotification<T> : INotification where T : Settings.Settings.Models.Settings, new()
    {
        protected BaseNotification(ISettingsService<T> settings, INotificationTemplatesRepository templateRepo)
        {
            Settings = settings;
            TemplateRepository = templateRepo;
        }
        
        protected ISettingsService<T> Settings { get; }
        protected INotificationTemplatesRepository TemplateRepository { get; }
        public abstract string NotificationName { get; }

        public async Task NotifyAsync(NotificationOptions model)
        {
            var configuration = GetConfiguration();
            await NotifyAsync(model, configuration);
        }

        public async Task NotifyAsync(NotificationOptions model, Settings.Settings.Models.Settings settings)
        {
            if (settings == null) await NotifyAsync(model);
            
            var notificationSettings = (T)settings;

            if (!ValidateConfiguration(notificationSettings))
            {
                return;
            }
            try
            {
                switch (model.NotificationType)
                {
                    case NotificationType.NewRequest:
                        await NewRequest(model, notificationSettings);
                        break;
                    case NotificationType.Issue:
                        await Issue(model, notificationSettings);
                        break;
                    case NotificationType.RequestAvailable:
                        await AvailableRequest(model, notificationSettings);
                        break;
                    case NotificationType.RequestApproved:
                        await RequestApproved(model, notificationSettings);
                        break;
                    case NotificationType.AdminNote:
                        throw new NotImplementedException();

                    case NotificationType.Test:
                        await Test(model, notificationSettings);
                        break;
                    case NotificationType.RequestDeclined:
                        await RequestDeclined(model, notificationSettings);
                        break;
                    case NotificationType.ItemAddedToFaultQueue:
                        await AddedToRequestQueue(model, notificationSettings);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (NotImplementedException)
            {
                // Do nothing, it's not implimented meaning it might not be ready or even used
            }
        }

        private T GetConfiguration()
        {
            var settings = Settings.GetSettings();
            return settings;
        }


        protected abstract bool ValidateConfiguration(T settings);
        protected abstract Task NewRequest(NotificationOptions model, T settings);
        protected abstract Task Issue(NotificationOptions model, T settings);
        protected abstract Task AddedToRequestQueue(NotificationOptions model, T settings);
        protected abstract Task RequestDeclined(NotificationOptions model, T settings);
        protected abstract Task RequestApproved(NotificationOptions model, T settings);
        protected abstract Task AvailableRequest(NotificationOptions model, T settings);
        protected abstract Task Send(NotificationMessage model, T settings);
        protected abstract Task Test(NotificationOptions model, T settings);
    }
}