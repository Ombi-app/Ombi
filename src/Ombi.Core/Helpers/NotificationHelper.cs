using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Notifications;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core
{
    public class NotificationHelper : INotificationHelper
    {
        public async Task NewRequest(FullBaseRequest model)
        {
            var notificationModel = new NotificationOptions
            {
                RequestId = model.Id,
                DateTime = DateTime.Now,
                NotificationType = NotificationType.NewRequest,
                RequestType = model.RequestType
            };
            await OmbiQuartz.TriggerJob(nameof(INotificationService), "Notifications", new Dictionary<string, object>
            {
                {JobDataKeys.NotificationOptions, notificationModel}
            });
        }

        public async Task NewRequest(ChildRequests model)
        {
            var notificationModel = new NotificationOptions
            {
                RequestId = model.Id,
                DateTime = DateTime.Now,
                NotificationType = NotificationType.NewRequest,
                RequestType = model.RequestType
            }; 
            await OmbiQuartz.TriggerJob(nameof(INotificationService), "Notifications", new Dictionary<string, object>
            {
                {JobDataKeys.NotificationOptions, notificationModel}
            });
        }

        public async Task NewRequest(AlbumRequest model)
        {
            var notificationModel = new NotificationOptions
            {
                RequestId = model.Id,
                DateTime = DateTime.Now,
                NotificationType = NotificationType.NewRequest,
                RequestType = model.RequestType
            }; 
            await OmbiQuartz.TriggerJob(nameof(INotificationService), "Notifications", new Dictionary<string, object>
            {
                {JobDataKeys.NotificationOptions, notificationModel}
            });
        }


        public async Task Notify(MovieRequests model, NotificationType type)
        {
            var notificationModel = new NotificationOptions
            {
                RequestId = model.Id,
                DateTime = DateTime.Now,
                NotificationType = type,
                RequestType = model.RequestType,
                Recipient = model.RequestedUser?.Email ?? string.Empty
            };

            await OmbiQuartz.TriggerJob(nameof(INotificationService), "Notifications", new Dictionary<string, object>
            {
                {JobDataKeys.NotificationOptions, notificationModel}
            });
        }
        public async Task Notify(ChildRequests model, NotificationType type)
        {
            var notificationModel = new NotificationOptions
            {
                RequestId = model.Id,
                DateTime = DateTime.Now,
                NotificationType = type,
                RequestType = model.RequestType,
                Recipient = model.RequestedUser?.Email ?? string.Empty
            };
            await OmbiQuartz.TriggerJob(nameof(INotificationService), "Notifications", new Dictionary<string, object>
            {
                {JobDataKeys.NotificationOptions, notificationModel}
            });
        }

        public async Task Notify(AlbumRequest model, NotificationType type)
        {
            var notificationModel = new NotificationOptions
            {
                RequestId = model.Id,
                DateTime = DateTime.Now,
                NotificationType = type,
                RequestType = model.RequestType,
                Recipient = model.RequestedUser?.Email ?? string.Empty
            };

            await OmbiQuartz.TriggerJob(nameof(INotificationService), "Notifications", new Dictionary<string, object>
            {
                {JobDataKeys.NotificationOptions, notificationModel}
            });
        }

        public async Task Notify(NotificationOptions model)
        {
            await OmbiQuartz.TriggerJob(nameof(INotificationService), "Notifications", new Dictionary<string, object>
            {
                {JobDataKeys.NotificationOptions, model}
            });
        }
    }
}