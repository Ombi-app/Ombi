#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SlackNotification.cs
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

namespace Ombi.Services.Notification
{
    public class DiscordNotification : BaseNotification<DiscordNotificationSettings>
    {
        public DiscordNotification(IDiscordApi api, ISettingsService<DiscordNotificationSettings> sn) : base(sn)
        {
            Api = api;
        }

        public override string NotificationName => "DiscordNotification";

        private IDiscordApi Api { get; }
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override bool ValidateConfiguration(DiscordNotificationSettings settings)
        {
            if (!settings.Enabled)
            {
                return false;
            }
            if (string.IsNullOrEmpty(settings.WebhookUrl))
            {
                return false;
            }
            try
            {
                var a = settings.Token;
                var b = settings.WebookId;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        protected override async Task NewRequest(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"{model.Title} has been requested by user: {model.User}";

            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        protected override async Task Issue(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"A new issue: {model.Body} has been reported by user: {model.User} for the title: {model.Title}";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"Hello! The user '{model.User}' has requested {model.Title} but it could not be added. This has been added into the requests queue and will keep retrying";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        protected override async Task RequestDeclined(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"Hello! Your request for {model.Title} has been declined, Sorry!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        protected override Task RequestApproved(NotificationModel model, DiscordNotificationSettings settings)
        {
            throw new NotImplementedException();
        }

        protected override Task AvailableRequest(NotificationModel model, DiscordNotificationSettings settings)
        {
            throw new NotImplementedException();
        }

        protected override async Task Send(NotificationMessage model, DiscordNotificationSettings settings)
        {
            try
            {
                await Api.SendMessageAsync(model.Message, settings.WebookId, settings.Token, settings.Username);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        protected override async Task Test(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"This is a test from Ombi, if you can see this then we have successfully pushed a notification!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }
    }
}