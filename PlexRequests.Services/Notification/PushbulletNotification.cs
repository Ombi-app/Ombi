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
            Settings = settings;
        }
        private IPushbulletApi PushbulletApi { get;  }
        private ISettingsService<PushbulletNotificationSettings> Settings { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();
        public string NotificationName => "PushbulletNotification";
        public bool Notify(string title, string requester)
        {
            var settings = GetSettings();

            if (!settings.Enabled)
            {
                return false;
            }

            var message = $"{title} has been requested by {requester}";
            var pushTitle = $"Plex Requests: {title}";
            try
            {
                var result = PushbulletApi.Push(settings.AccessToken, pushTitle, message, settings.DeviceIdentifier);
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

        private PushbulletNotificationSettings GetSettings()
        {
            return Settings.GetSettings();
        }
    }
}