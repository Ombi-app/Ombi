#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: AdminNotificationsModule.cs
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

using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using Nancy.Validation;

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Notification;
using PlexRequests.UI.Helpers;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class AdminNotificationsModule : BaseModule
    {
        public AdminNotificationsModule(ISettingsService<PlexRequestSettings> prService, ISettingsService<SlackNotificationSettings> slackSettings,
            INotificationService notify, ISlackApi slackApi) : base("admin", prService)
        {
            this.RequiresClaims(UserClaims.Admin);

            SlackSettings = slackSettings;
            NotificationService = notify;
            SlackApi = slackApi;

            Post["/testslacknotification"] = _ => TestSlackNotification();

            Get["/slacknotification"] = _ => SlackNotifications();
            Post["/slacknotification"] = _ => SaveSlackNotifications();
        }
        private ISettingsService<SlackNotificationSettings> SlackSettings { get; }
        private INotificationService NotificationService { get; }
        private ISlackApi SlackApi { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        private Response TestSlackNotification()
        {
            var settings = this.BindAndValidate<SlackNotificationSettings>();
            if (!ModelValidationResult.IsValid)
            {
                return Response.AsJson(ModelValidationResult.SendJsonError());
            }
            var notificationModel = new NotificationModel
            {
                NotificationType = NotificationType.Test,
                DateTime = DateTime.Now
            };
            try
            {
                NotificationService.Subscribe(new SlackNotification(SlackApi,SlackSettings));
                settings.Enabled = true;
                NotificationService.Publish(notificationModel, settings);
                Log.Info("Sent slack notification test");
            }
            catch (Exception e)
            {
                Log.Error(e,"Failed to subscribe and publish test Slack Notification");
            }
            finally
            {
                NotificationService.UnSubscribe(new SlackNotification(SlackApi, SlackSettings));
            }
            return Response.AsJson(new JsonResponseModel { Result = true, Message = "Successfully sent a test Slack Notification! If you do not receive it please check the logs." });
        }

        private Negotiator SlackNotifications()
        {
            var settings = SlackSettings.GetSettings();
            return View["Admin/SlackNotifications", settings];
        }

        private Response SaveSlackNotifications()
        {
            var settings = this.BindAndValidate<SlackNotificationSettings>();
            if (!ModelValidationResult.IsValid)
            {
                return Response.AsJson(ModelValidationResult.SendJsonError());
            }

            var result = SlackSettings.SaveSettings(settings);
            if (settings.Enabled)
            {
                NotificationService.Subscribe(new SlackNotification(SlackApi, SlackSettings));
            }
            else
            {
                NotificationService.UnSubscribe(new SlackNotification(SlackApi, SlackSettings));
            }

            Log.Info("Saved slack settings, result: {0}", result);
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true, Message = "Successfully Updated the Settings for Slack Notifications!" }
                : new JsonResponseModel { Result = false, Message = "Could not update the settings, take a look at the logs." });
        }
    }
}