using System;
using Hangfire;
using Ombi.Core.Models.Requests;
using Ombi.Core.Notifications;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Store.Entities;
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
                Title = model.Title,
                RequestedUser = model.RequestedUser.Username,
                DateTime = DateTime.Now,
                NotificationType = NotificationType.NewRequest,
                RequestType = model.RequestType,
                ImgSrc = model.RequestType == RequestType.Movie
                    ? $"https://image.tmdb.org/t/p/w300/{model.PosterPath}"
                    : model.PosterPath
            };
            BackgroundJob.Enqueue(() => NotificationService.Publish(notificationModel));

        }
    }
}