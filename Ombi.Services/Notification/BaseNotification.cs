#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: BaseNotification.cs
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
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;
using Ombi.Services.Interfaces;

namespace Ombi.Services.Notification
{
    public abstract class BaseNotification<T,U> : INotification where T : Settings, new() where U : new()
    {
        protected BaseNotification(ISettingsService<T> settings)
        {
            Settings = settings;
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        protected ISettingsService<T> Settings { get; }
        public abstract string NotificationName { get; }

        public async Task NotifyAsync(NotificationModel model)
        {
            var configuration = GetConfiguration();
            await NotifyAsync(model, configuration);
        }

        public async Task NotifyAsync(NotificationModel model, Settings settings)
        {
            if (settings == null) await NotifyAsync(model);

            var notificationSettings = (T)settings;

            if (!ValidateConfiguration(notificationSettings))
            {
                return;
            }

            switch (model.NotificationType)
            {
                case NotificationType.NewRequest:
                    await EmailNewRequest(model, notificationSettings);
                    break;
                case NotificationType.Issue:
                    await EmailIssue(model, notificationSettings);
                    break;
                case NotificationType.RequestAvailable:
                    await EmailAvailableRequest(model, notificationSettings);
                    break;
                case NotificationType.RequestApproved:
                    await EmailRequestApproved(model, notificationSettings);
                    break;
                case NotificationType.AdminNote:
                    throw new NotImplementedException();

                case NotificationType.Test:
                    await EmailTest(model, notificationSettings);
                    break;
                case NotificationType.RequestDeclined:
                    await EmailRequestDeclined(model, notificationSettings);
                    break;
                case NotificationType.ItemAddedToFaultQueue:
                    await EmailAddedToRequestQueue(model, notificationSettings);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private T GetConfiguration()
        {
            var settings = Settings.GetSettings();
            return settings;
        }


        protected abstract bool ValidateConfiguration(T settings);
        protected abstract Task EmailNewRequest(NotificationModel model, T settings);
        protected abstract Task EmailIssue(NotificationModel model, T settings);
        protected abstract Task EmailAddedToRequestQueue(NotificationModel model, T settings);
        protected abstract Task EmailRequestDeclined(NotificationModel model, T settings);
        protected abstract Task EmailRequestApproved(NotificationModel model, T settings);
        protected abstract Task EmailAvailableRequest(NotificationModel model, T settings);
        protected abstract Task Send(U message, T settings);
        protected abstract Task EmailTest(NotificationModel model, T settings);

    }
}