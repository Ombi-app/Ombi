using System;
using System.Threading.Tasks;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models;
using Ombi.Notifications.Models;

namespace Ombi.Notifications
{
    public abstract class BaseNotification<T> : INotification where T : Settings.Settings.Models.Settings, new()
    {
        protected BaseNotification(ISettingsService<T> settings)
        {
            Settings = settings;
        }
        
        protected ISettingsService<T> Settings { get; }
        public abstract string NotificationName { get; }

        public async Task NotifyAsync(NotificationModel model)
        {
            var configuration = GetConfiguration();
            await NotifyAsync(model, configuration);
        }

        public async Task NotifyAsync(NotificationModel model, Settings.Settings.Models.Settings settings)
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
        protected abstract Task NewRequest(NotificationModel model, T settings);
        protected abstract Task Issue(NotificationModel model, T settings);
        protected abstract Task AddedToRequestQueue(NotificationModel model, T settings);
        protected abstract Task RequestDeclined(NotificationModel model, T settings);
        protected abstract Task RequestApproved(NotificationModel model, T settings);
        protected abstract Task AvailableRequest(NotificationModel model, T settings);
        protected abstract Task Send(NotificationMessage model, T settings);
        protected abstract Task Test(NotificationModel model, T settings);
    }
}