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
using PlexRequests.Core.SettingModels;
using PlexRequests.Services.Interfaces;

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
        private PushoverNotificationSettings Settings => GetSettings();

        private static Logger Log = LogManager.GetCurrentClassLogger();
        public string NotificationName => "PushoverNotification";
        public async Task NotifyAsync(NotificationModel model)
        {
            if (!ValidateConfiguration())
            {
                return;
            }

            switch (model.NotificationType)
            {
                case NotificationType.NewRequest:
                    await PushNewRequestAsync(model);
                    break;
                case NotificationType.Issue:
                    await PushIssueAsync(model);
                    break;
                case NotificationType.RequestAvailable:
                    break;
                case NotificationType.RequestApproved:
                    break;
                case NotificationType.AdminNote:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool ValidateConfiguration()
        {
            if (!Settings.Enabled)
            {
                return false;
            }
            if (string.IsNullOrEmpty(Settings.AccessToken) || string.IsNullOrEmpty(Settings.UserToken))
            {
                return false;
            }
            return true;
        }

        private PushoverNotificationSettings GetSettings()
        {
            return SettingsService.GetSettings();
        }

        private async Task PushNewRequestAsync(NotificationModel model)
        {
            var message = $"Plex Requests: {model.Title} has been requested by user: {model.User}";
            try
            {
                var result = await PushoverApi.PushAsync(Settings.AccessToken, message, Settings.UserToken);
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

        private async Task PushIssueAsync(NotificationModel model)
        {
            var message = $"Plex Requests: A new issue: {model.Body} has been reported by user: {model.User} for the title: {model.Title}";
            try
            {
                var result = await PushoverApi.PushAsync(Settings.AccessToken, message, Settings.UserToken);
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