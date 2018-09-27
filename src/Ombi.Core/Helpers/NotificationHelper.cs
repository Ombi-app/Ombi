using System;
using Hangfire;
using Ombi.Core.Notifications;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core
{
    public class NotificationHelper : INotificationHelper
    {
        public NotificationHelper(INotificationService service)
        {
            NotificationService = service;
        }
        private INotificationService NotificationService { get; }

        public void NewRequest(FullBaseRequest model)
        {
            var notificationModel = new NotificationOptions
            {
                RequestId = model.Id,
                DateTime = DateTime.Now,
                NotificationType = NotificationType.NewRequest,
                RequestType = model.RequestType
            };
            BackgroundJob.Enqueue(() => NotificationService.Publish(notificationModel));

        }

        public void NewRequest(ChildRequests model)
        {
            var notificationModel = new NotificationOptions
            {
                RequestId = model.Id,
                DateTime = DateTime.Now,
                NotificationType = NotificationType.NewRequest,
                RequestType = model.RequestType
            };
            BackgroundJob.Enqueue(() => NotificationService.Publish(notificationModel));
        }

        public void NewRequest(AlbumRequest model)
        {
            var notificationModel = new NotificationOptions
            {
                RequestId = model.Id,
                DateTime = DateTime.Now,
                NotificationType = NotificationType.NewRequest,
                RequestType = model.RequestType
            };
            BackgroundJob.Enqueue(() => NotificationService.Publish(notificationModel));
        }


        public void Notify(MovieRequests model, NotificationType type)
        {
            var notificationModel = new NotificationOptions
            {
                RequestId = model.Id,
                DateTime = DateTime.Now,
                NotificationType = type,
                RequestType = model.RequestType,
                Recipient = model.RequestedUser?.Email ?? string.Empty
            };
            
            BackgroundJob.Enqueue(() => NotificationService.Publish(notificationModel));
        }
        public void Notify(ChildRequests model, NotificationType type)
        {
            var notificationModel = new NotificationOptions
            {
                RequestId = model.Id,
                DateTime = DateTime.Now,
                NotificationType = type,
                RequestType = model.RequestType,
                Recipient = model.RequestedUser?.Email ?? string.Empty
            };
            BackgroundJob.Enqueue(() => NotificationService.Publish(notificationModel));
        }

        public void Notify(AlbumRequest model, NotificationType type)
        {
            var notificationModel = new NotificationOptions
            {
                RequestId = model.Id,
                DateTime = DateTime.Now,
                NotificationType = type,
                RequestType = model.RequestType,
                Recipient = model.RequestedUser?.Email ?? string.Empty
            };

            BackgroundJob.Enqueue(() => NotificationService.Publish(notificationModel));
        }
    }
}