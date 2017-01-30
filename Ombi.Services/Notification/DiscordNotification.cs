﻿#region Copyright
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
using Ombi.Api.Models.Notifications;
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;
using Ombi.Services.Interfaces;

namespace Ombi.Services.Notification
{
    public class DiscordNotification : INotification
    {
        public DiscordNotification(IDiscordApi api, ISettingsService<DiscordNotificationSettings> sn)
        {
            Api = api;
            Settings = sn;
        }

        public string NotificationName => "DiscordNotification";

        private IDiscordApi Api { get; }
        private ISettingsService<DiscordNotificationSettings> Settings { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();


        public async Task NotifyAsync(NotificationModel model)
        {
            var settings = Settings.GetSettings();

            await NotifyAsync(model, settings);
        }

        public async Task NotifyAsync(NotificationModel model, Settings settings)
        {
            if (settings == null) await NotifyAsync(model);

            var pushSettings = (DiscordNotificationSettings)settings;
            if (!ValidateConfiguration(pushSettings))
            {
                Log.Error("Settings for Slack was not correct, we cannot push a notification");
                return;
            }

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
                    await PushTest(pushSettings);
                    break;
                case NotificationType.RequestDeclined:
                    await PushRequestDeclinedAsync(model, pushSettings);
                    break;
                case NotificationType.ItemAddedToFaultQueue:
                    await PushFaultQueue(model, pushSettings);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task PushNewRequestAsync(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"{model.Title} has been requested by user: {model.User}";
            await Push(settings, message);
        }

        private async Task PushRequestDeclinedAsync(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"Hello! Your request for {model.Title} has been declined, Sorry!";
            await Push(settings, message);
        }

        private async Task PushIssueAsync(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"A new issue: {model.Body} has been reported by user: {model.User} for the title: {model.Title}";
            await Push(settings, message);
        }

        private async Task PushTest(DiscordNotificationSettings settings)
        {
            var message = $"This is a test from Ombi, if you can see this then we have successfully pushed a notification!";
            await Push(settings, message);
        }

        private async Task PushFaultQueue(NotificationModel model, DiscordNotificationSettings settings)
        {
            var message = $"Hello! The user '{model.User}' has requested {model.Title} but it could not be added. This has been added into the requests queue and will keep retrying";
            await Push(settings, message);
        }

        private async Task Push(DiscordNotificationSettings config, string message)
        {
            try
            {
                await Api.SendMessageAsync(message, config.WebookId, config.Token, config.Username);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private bool ValidateConfiguration(DiscordNotificationSettings settings)
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
    }
}