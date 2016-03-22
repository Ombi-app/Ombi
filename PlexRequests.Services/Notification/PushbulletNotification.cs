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

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.Services.Notification
{
    public class PushbulletNotification : INotification
    {
        public PushbulletNotification(IPushbulletApi pushbulletApi, ISettingsService<PushbulletNotificationSettings> settings)
        {
            PushbulletApi = pushbulletApi;
            SettingsService = settings;
        }
        private IPushbulletApi PushbulletApi { get;  }
        private ISettingsService<PushbulletNotificationSettings> SettingsService { get; }
        private PushbulletNotificationSettings Settings => GetSettings();

        private static Logger Log = LogManager.GetCurrentClassLogger();
        public string NotificationName => "PushbulletNotification";
        public bool Notify(NotificationModel model)
        {
            if (!ValidateConfiguration())
            {
                return false;
            }

            switch (model.NotificationType)
            {
                case NotificationType.NewRequest:
                    return PushNewRequest(model);
 
                case NotificationType.Issue:
                    return PushIssue(model);

                case NotificationType.RequestAvailable:
                    break;
                case NotificationType.RequestApproved:
                    break;
                case NotificationType.AdminNote:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;

        }

        private bool ValidateConfiguration()
        {
            if (!Settings.Enabled)
            {
                return false;
            }
            if (string.IsNullOrEmpty(Settings.AccessToken))
            {
                return false;
            }
            return true;
        }

        private PushbulletNotificationSettings GetSettings()
        {
            return SettingsService.GetSettings();
        }

        private bool PushNewRequest(NotificationModel model)
        {
            var message = $"{model.Title} has been requested by user: {model.User}";
            var pushTitle = $"Plex Requests: {model.Title} has been requested!";
            try
            {
                var result = PushbulletApi.Push(Settings.AccessToken, pushTitle, message, Settings.DeviceIdentifier);
                if (result != null)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
            return false;
        }

        private bool PushIssue(NotificationModel model)
        {
            var message = $"A new issue: {model.Body} has been reported by user: {model.User} for the title: {model.Title}";
            var pushTitle = $"Plex Requests: A new issue has been reported for {model.Title}";
            try
            {
                var result = PushbulletApi.Push(Settings.AccessToken, pushTitle, message, Settings.DeviceIdentifier);
                if (result != null)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
            return false;
        }
    }
}