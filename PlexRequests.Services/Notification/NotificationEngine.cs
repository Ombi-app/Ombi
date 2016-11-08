#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: NotificationEngine.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NLog.Fluent;
using PlexRequests.Api;
using PlexRequests.Api.Interfaces;
using PlexRequests.Core.Models;
using PlexRequests.Services.Interfaces;
using PlexRequests.Store;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;

namespace PlexRequests.Services.Notification
{
    public class NotificationEngine : INotificationEngine
    {
        public NotificationEngine(IPlexApi p, IRepository<UsersToNotify> repo, INotificationService service)
        {
            PlexApi = p;
            UserNotifyRepo = repo;
            Notification = service;
        }

        private IPlexApi PlexApi { get; }
        private IRepository<UsersToNotify> UserNotifyRepo { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private INotificationService Notification { get; }

        public async Task NotifyUsers(IEnumerable<RequestedModel> modelChanged, string apiKey, NotificationType type)
        {
            try
            {
                var plexUser = PlexApi.GetUsers(apiKey);
                var userAccount = PlexApi.GetAccount(apiKey);

                var adminUsername = userAccount.Username ?? string.Empty;

                var users = UserNotifyRepo.GetAll().ToList();
                Log.Debug("Notifying Users Count {0}", users.Count);
                foreach (var model in modelChanged)
                {
                    var selectedUsers = users.Select(x => x.Username).Intersect(model.RequestedUsers, StringComparer.CurrentCultureIgnoreCase);
                    foreach (var user in selectedUsers)
                    {
                        Log.Info("Notifying user {0}", user);
                        if (user.Equals(adminUsername, StringComparison.CurrentCultureIgnoreCase))
                        {
                            Log.Info("This user is the Plex server owner");
                            await PublishUserNotification(userAccount.Username, userAccount.Email, model.Title, model.PosterPath, type);
                            return;
                        }

                        var email = plexUser.User.FirstOrDefault(x => x.Username.Equals(user, StringComparison.CurrentCultureIgnoreCase));
                        if (email == null)
                        {
                            Log.Info("There is no email address for this Plex user, cannot send notification");
                            // We do not have a plex user that requested this!
                            continue;
                        }

                        Log.Info("Sending notification to: {0} at: {1}, for title: {2}", email.Username, email.Email, model.Title);
                        await PublishUserNotification(email.Username, email.Email, model.Title, model.PosterPath, type);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public async Task NotifyUsers(RequestedModel model, string apiKey, NotificationType type)
        {
            try
            {
                var plexUser = PlexApi.GetUsers(apiKey);
                var userAccount = PlexApi.GetAccount(apiKey);

                var adminUsername = userAccount.Username ?? string.Empty;

                var users = UserNotifyRepo.GetAll().ToList();
                Log.Debug("Notifying Users Count {0}", users.Count);

                var selectedUsers = users.Select(x => x.Username).Intersect(model.RequestedUsers, StringComparer.CurrentCultureIgnoreCase);
                foreach (var user in selectedUsers)
                {
                    Log.Info("Notifying user {0}", user);
                    if (user.Equals(adminUsername, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Log.Info("This user is the Plex server owner");
                        await PublishUserNotification(userAccount.Username, userAccount.Email, model.Title, model.PosterPath, type);
                        return;
                    }

                    var email = plexUser.User.FirstOrDefault(x => x.Username.Equals(user, StringComparison.CurrentCultureIgnoreCase));
                    if (email == null)
                    {
                        Log.Info("There is no email address for this Plex user, cannot send notification");
                        // We do not have a plex user that requested this!
                        continue;
                    }

                    Log.Info("Sending notification to: {0} at: {1}, for title: {2}", email.Username, email.Email, model.Title);
                    await PublishUserNotification(email.Username, email.Email, model.Title, model.PosterPath, type);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private async Task PublishUserNotification(string username, string email, string title, string img, NotificationType type)
        {
            var notificationModel = new NotificationModel
            {
                User = username,
                UserEmail = email,
                NotificationType = type,
                Title = title,
                ImgSrc = img
            };

            // Send the notification to the user.
            await Notification.Publish(notificationModel);
        }
    }
}