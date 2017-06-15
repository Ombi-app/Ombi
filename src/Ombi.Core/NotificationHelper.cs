using System;
using Hangfire;
using Ombi.Core.Models.Requests;
using Ombi.Core.Notifications;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Store.Entities;

namespace Ombi.Core
{
    public class NotificationHelper : INotificationHelper
    {
        public NotificationHelper(INotificationService service)
        {
            NotificationService = service;
        }
        private INotificationService NotificationService { get; }

        public void NewRequest(BaseRequestModel model)
        {
            var notificationModel = new NotificationOptions
            {
                Title = model.Title,
                RequestedUser = model.RequestedUser,
                DateTime = DateTime.Now,
                NotificationType = NotificationType.NewRequest,
                RequestType = model.Type,
                ImgSrc = model.Type == RequestType.Movie
                    ? $"https://image.tmdb.org/t/p/w300/{model.PosterPath}"
                    : model.PosterPath
            };
            BackgroundJob.Enqueue(() => NotificationService.Publish(notificationModel));

        }
    }
}