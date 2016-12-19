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
using Ombi.Api.Interfaces;
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;
using Ombi.Services.Interfaces;
using Ombi.Store;

namespace Ombi.Services.Notification
{
    public class PushbulletNotification : INotification
    {
        public PushbulletNotification(IPushbulletApi pushbulletApi, ISettingsService<PushbulletNotificationSettings> settings)
        {
            PushbulletApi = pushbulletApi;
            SettingsService = settings;
        }
        private IPushbulletApi PushbulletApi { get; }
        private ISettingsService<PushbulletNotificationSettings> SettingsService { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();
        public string NotificationName => "PushbulletNotification";
        public async Task NotifyAsync(NotificationModel model)
        {
            var configuration = GetSettings();
            await NotifyAsync(model, configuration);
        }

        public async Task NotifyAsync(NotificationModel model, Settings settings)
        {
            if (settings == null) await NotifyAsync(model);

            var pushSettings = (PushbulletNotificationSettings)settings;

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
                    await PushTestAsync(pushSettings);
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

        private bool ValidateConfiguration(PushbulletNotificationSettings settings)
        {
            if (!settings.Enabled)
            {
                return false;
            }
            if (string.IsNullOrEmpty(settings.AccessToken))
            {
                return false;
            }
            return true;
        }

        private PushbulletNotificationSettings GetSettings()
        {
            return SettingsService.GetSettings();
        }

        private async Task PushNewRequestAsync(NotificationModel model, PushbulletNotificationSettings settings)
        {
            var message = $"The {model.RequestType.GetString()?.ToLower()} '{model.Title}' has been requested by user: {model.User}";
            var pushTitle = $"Plex Requests: The {model.RequestType.GetString()?.ToLower()} {model.Title} has been requested!";
            await Push(settings, message, pushTitle);
        }

        private async Task PushIssueAsync(NotificationModel model, PushbulletNotificationSettings settings)
        {
            var message = $"A new issue: {model.Body} has been reported by user: {model.User} for the title: {model.Title}";
            var pushTitle = $"Plex Requests: A new issue has been reported for {model.Title}";
            await Push(settings, message, pushTitle);
        }

        private async Task PushTestAsync(PushbulletNotificationSettings settings)
        {
            var message = "This is just a test! Success!";
            var pushTitle = "Plex Requests: Test Message!";
            await Push(settings, message, pushTitle);
        }

        private async Task PushFaultQueue(NotificationModel model, PushbulletNotificationSettings settings)
        {
            var message = $"Hello! The user '{model.User}' has requested {model.Title} but it could not be added. This has been added into the requests queue and will keep retrying";
            var pushTitle = $"Plex Requests: The {model.RequestType.GetString()?.ToLower()} {model.Title} has been requested but could not be added!";
            await Push(settings, message, pushTitle);
        }

        private async Task Push(PushbulletNotificationSettings settings, string message, string title)
        {
            try
            {
                var result = await PushbulletApi.PushAsync(settings.AccessToken, title, message, settings.DeviceIdentifier);
                if (result != null)
                {
                    Log.Error("Pushbullet api returned a null value, the notification did not get pushed");
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}