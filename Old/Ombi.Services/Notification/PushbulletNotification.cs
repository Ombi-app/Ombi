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
using Ombi.Core.SettingModels;
using Ombi.Store;

namespace Ombi.Services.Notification
{
    public class PushbulletNotification : BaseNotification<PushbulletNotificationSettings>
    {
        public PushbulletNotification(IPushbulletApi pushbulletApi, ISettingsService<PushbulletNotificationSettings> settings) : base(settings)
        {
            PushbulletApi = pushbulletApi;
        }
        private IPushbulletApi PushbulletApi { get; }
        private ISettingsService<PushbulletNotificationSettings> SettingsService { get; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public override string NotificationName => "PushbulletNotification";
        
        protected override  bool ValidateConfiguration(PushbulletNotificationSettings settings)
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

        protected override async Task NewRequest(NotificationModel model, PushbulletNotificationSettings settings)
        {
            var message = $"The {model.RequestType.GetString()?.ToLower()} '{model.Title}' has been requested by user: {model.User}";
            var pushTitle = $"Ombi: The {model.RequestType.GetString()?.ToLower()} {model.Title} has been requested!";
            var notification = new NotificationMessage
            {
                Message = message,
                Subject = pushTitle
            };
            await Send(notification, settings);
        }
        

        protected override async Task Issue(NotificationModel model, PushbulletNotificationSettings settings)
        {
            var message = $"A new issue: {model.Body} has been reported by user: {model.User} for the title: {model.Title}";
            var pushTitle = $"Ombi: A new issue has been reported for {model.Title}";
            var notification = new NotificationMessage
            {
                Message = message,
                Subject = pushTitle
            };
            await Send(notification, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationModel model, PushbulletNotificationSettings settings)
        {
           
            var message = $"Hello!The user '{model.User}' has requested { model.Title} but it could not be added. This has been added into the requests queue and will keep retrying";
            var pushTitle = $"Ombi: A request could not be added.";
            var notification = new NotificationMessage
            {
                Message = message,
                Subject = pushTitle
            };
            await Send(notification, settings);
        }

        protected override Task RequestDeclined(NotificationModel model, PushbulletNotificationSettings settings)
        {
            throw new NotImplementedException();
        }

        protected override Task RequestApproved(NotificationModel model, PushbulletNotificationSettings settings)
        {
            throw new NotImplementedException();
        }

        protected override Task AvailableRequest(NotificationModel model, PushbulletNotificationSettings settings)
        {
            throw new NotImplementedException();
        }

        protected override async Task Send(NotificationMessage model, PushbulletNotificationSettings settings)
        {
            try
            {
                var result = await PushbulletApi.PushAsync(settings.AccessToken, model.Subject, model.Message, settings.DeviceIdentifier);
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

        protected override async Task Test(NotificationModel model, PushbulletNotificationSettings settings)
        {
            var message = "This is just a test! Success!";
            var pushTitle = "Ombi: Test Message!";
            var notification = new NotificationMessage
            {
                Message = message,
                Subject = pushTitle
            };
            await Send(notification,settings);
        }
    }
}