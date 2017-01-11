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
using Ombi.Api.Interfaces;
using Ombi.Core.Models;
using Ombi.Core.Users;
using Ombi.Helpers.Permissions;
using Ombi.Services.Interfaces;
using Ombi.Store;
using Ombi.Store.Models;
using Ombi.Store.Repository;

namespace Ombi.Services.Notification
{
    public class NotificationEngine : INotificationEngine
    {
        public NotificationEngine(IPlexApi p, IRepository<UsersToNotify> repo, INotificationService service, IUserHelper userHelper)
        {
            PlexApi = p;
            UserNotifyRepo = repo;
            Notification = service;
            UserHelper = userHelper;
        }

        private IPlexApi PlexApi { get; }
        private IRepository<UsersToNotify> UserNotifyRepo { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private INotificationService Notification { get; }
        private IUserHelper UserHelper { get; }

        public async Task NotifyUsers(IEnumerable<RequestedModel> modelChanged, string apiKey, NotificationType type)
        {
            try
            {
                var plexUser = PlexApi.GetUsers(apiKey);
                var userAccount = PlexApi.GetAccount(apiKey);

                var adminUsername = userAccount.Username ?? string.Empty;
                
                var users = UserHelper.GetUsersWithFeature(Features.RequestAddedNotification).ToList();
                Log.Debug("Notifying Users Count {0}", users.Count);
                foreach (var model in modelChanged)
                {
                    var selectedUsers = new List<string>();

                    foreach (var u in users)
                    {
                        var requestUser = model.RequestedUsers.FirstOrDefault(
                                x => x.Equals(u.Username, StringComparison.CurrentCultureIgnoreCase) || x.Equals(u.UserAlias, StringComparison.CurrentCultureIgnoreCase));
                        if (string.IsNullOrEmpty(requestUser))
                        {
                            continue;
                        }

                        // Make sure we do not already have the user
                        if (!selectedUsers.Contains(requestUser))
                        {
                            selectedUsers.Add(requestUser);
                        }
                    }

                    //var selectedUsers = users.Select(x => x.Username).Intersect(model.RequestedUsers, StringComparer.CurrentCultureIgnoreCase);
                    foreach (var user in selectedUsers)
                    {
                        Log.Info("Notifying user {0}", user);
                        if (user.Equals(adminUsername, StringComparison.CurrentCultureIgnoreCase))
                        {
                            Log.Info("This user is the Plex server owner");
                            await PublishUserNotification(userAccount.Username, userAccount.Email, model.Title, model.PosterPath, type, model.Type);
                            return;
                        }

                        var email = plexUser.User.FirstOrDefault(x => x.Username.Equals(user, StringComparison.CurrentCultureIgnoreCase));
                        if (string.IsNullOrEmpty(email?.Email))
                        {
                            Log.Info("There is no email address for this Plex user, cannot send notification");
                            // We do not have a plex user that requested this!
                            continue;
                        }

                        Log.Info("Sending notification to: {0} at: {1}, for title: {2}", email.Username, email.Email, model.Title);
                        await PublishUserNotification(email.Username, email.Email, model.Title, model.PosterPath, type, model.Type);
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

                var users = UserHelper.GetUsersWithFeature(Features.RequestAddedNotification).ToList();
                Log.Debug("Notifying Users Count {0}", users.Count);

                var selectedUsers = users.Select(x => x.Username).Intersect(model.RequestedUsers, StringComparer.CurrentCultureIgnoreCase);
                foreach (var user in selectedUsers)
                {
                    Log.Info("Notifying user {0}", user);
                    if (user.Equals(adminUsername, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Log.Info("This user is the Plex server owner");
                        await PublishUserNotification(userAccount.Username, userAccount.Email, model.Title, model.PosterPath, type, model.Type);
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
                    await PublishUserNotification(email.Username, email.Email, model.Title, model.PosterPath, type, model.Type);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private async Task PublishUserNotification(string username, string email, string title, string img, NotificationType type, RequestType requestType)
        {
            var notificationModel = new NotificationModel
            {
                User = username,
                UserEmail = email,
                NotificationType = type,
                Title = title,
                ImgSrc = requestType == RequestType.Movie ? $"https://image.tmdb.org/t/p/w300/{img}" : img
            };

            // Send the notification to the user.
            await Notification.Publish(notificationModel);
        }
    }
}