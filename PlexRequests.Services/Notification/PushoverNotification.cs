#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PushbulletNotification.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion
using System;
using System.Threading.Tasks;

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.Models;
using PlexRequests.Core.SettingModels;
using PlexRequests.Services.Interfaces;
using PlexRequests.Store;

namespace PlexRequests.Services.Notification
{
    public class PushoverNotification : INotification
    {
        public PushoverNotification(IPushoverApi pushoverApi, ISettingsService<PushoverNotificationSettings> settings)
        {
            PushoverApi = pushoverApi;
            SettingsService = settings;
        }
        private IPushoverApi PushoverApi { get;  }
        private ISettingsService<PushoverNotificationSettings> SettingsService { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();
        public string NotificationName => "PushoverNotification";
        public async Task NotifyAsync(NotificationModel model)
        {
            var configuration = GetSettings();
            await NotifyAsync(model, configuration);
        }

        public async Task NotifyAsync(NotificationModel model, Settings settings)
        {
            if (settings == null) await NotifyAsync(model);

            var pushSettings = (PushoverNotificationSettings)settings;

            if (!ValidateConfiguration(pushSettings)) return;

            switch (model.NotificationType)
            {
                case NotificationType.NewRequest:
                    await PushNewRequestAsync(model, pushSettings);
                    break;
                case NotificationType.Issue:
                    await PushIssueAsync(model, pushSettings);
                    break;
                case NotificationType.RequestAvailable:
                    break;
                case NotificationType.RequestApproved:
                    break;
                case NotificationType.AdminNote:
                    break;
                case NotificationType.Test:
                    await PushTestAsync(model, pushSettings);
                    break;
                case NotificationType.RequestDeclined:
                    break;
                case NotificationType.ItemAddedToFaultQueue:
                    await PushFaultQueue(model, pushSettings);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool ValidateConfiguration(PushoverNotificationSettings settings)
        {
            if (!settings.Enabled)
            {
                return false;
            }
            if (string.IsNullOrEmpty(settings.AccessToken) || string.IsNullOrEmpty(settings.UserToken))
            {
                return false;
            }
            return true;
        }

        private PushoverNotificationSettings GetSettings()
        {
            return SettingsService.GetSettings();
        }

        private async Task PushNewRequestAsync(NotificationModel model, PushoverNotificationSettings settings)
        {
            var message = $"Plex Requests: The {model.RequestType.GetString()?.ToLower()} '{model.Title}' has been requested by user: {model.User}";
            await Push(settings, message);
        }

        private async Task PushIssueAsync(NotificationModel model, PushoverNotificationSettings settings)
        {
            var message = $"Plex Requests: A new issue: {model.Body} has been reported by user: {model.User} for the title: {model.Title}";
            await Push(settings, message);
        }

        private async Task PushTestAsync(NotificationModel model, PushoverNotificationSettings settings)
        {
            var message = $"Plex Requests: Test Message!";
            await Push(settings, message);
        }

        private async Task PushFaultQueue(NotificationModel model, PushoverNotificationSettings settings)
        {
            var message = $"Hello! The user '{model.User}' has requested {model.Title} but it could not be added. This has been added into the requests queue and will keep retrying";
            await Push(settings, message);
        }

        private async Task Push(PushoverNotificationSettings settings, string message)
        {
            try
            {
                var result = await PushoverApi.PushAsync(settings.AccessToken, message, settings.UserToken);
                if (result?.status != 1)
                {
                    Log.Error("Pushover api returned a status that was not 1, the notification did not get pushed");
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}