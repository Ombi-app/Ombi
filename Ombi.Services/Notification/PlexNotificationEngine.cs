#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexNotificationEngine.cs
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
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;
using Ombi.Core.Users;
using Ombi.Helpers.Permissions;
using Ombi.Services.Interfaces;
using Ombi.Store;
using Ombi.Store.Models;
using Ombi.Store.Repository;

namespace Ombi.Services.Notification
{
    public class PlexNotificationEngine : IPlexNotificationEngine
    {
        public PlexNotificationEngine(IPlexApi p, IRepository<UsersToNotify> repo, INotificationService service, IUserHelper userHelper, ISettingsService<PlexSettings> ps)
        {
            PlexApi = p;
            UserNotifyRepo = repo;
            Notification = service;
            UserHelper = userHelper;
            PlexSettings = ps;
        }

        private IPlexApi PlexApi { get; }
        private IRepository<UsersToNotify> UserNotifyRepo { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private INotificationService Notification { get; }
        private IUserHelper UserHelper { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }

        public async Task NotifyUsers(IEnumerable<RequestedModel> modelChanged, NotificationType type)
        {
            try
            {
                var settings = await PlexSettings.GetSettingsAsync();
                var plexUser = PlexApi.GetUsers(settings.PlexAuthToken);
                var userAccount = PlexApi.GetAccount(settings.PlexAuthToken);

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
                    
                    foreach (var user in selectedUsers)
                    {
                        Log.Info("Notifying user {0}", user);
                        if (user.Equals(adminUsername, StringComparison.CurrentCultureIgnoreCase))
                        {
                            Log.Info("This user is the Plex server owner");
                            await PublishUserNotification(userAccount.Username, userAccount.Email, model.Title, model.PosterPath, type, model.Type);
                            return;
                        }

                        UserHelperModel localUser = null;
                            //users.FirstOrDefault( x =>
                            //        x.Username.Equals(user, StringComparison.CurrentCultureIgnoreCase) ||
                            //        x.UserAlias.Equals(user, StringComparison.CurrentCultureIgnoreCase));
                            
                        foreach (var userHelperModel in users)
                        {
                            if (!string.IsNullOrEmpty(userHelperModel.Username))
                            {
                                if (userHelperModel.Username.Equals(user, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    localUser = userHelperModel;
                                    break;
                                }
                            }
                            if (!string.IsNullOrEmpty(userHelperModel.UserAlias))
                            {
                                if (userHelperModel.UserAlias.Equals(user, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    localUser = userHelperModel;
                                    break;
                                }
                            }
                        }


                        // So if the request was from an alias, then we need to use the local user (since that contains the alias).
                        // If we do not have a local user, then we should be using the Plex user if that user exists.
                        // This will execute most of the time since Plex and Local users will most always be in the database.
                        if (localUser != null)
                        {
                            if (string.IsNullOrEmpty(localUser?.EmailAddress))
                            {
                                Log.Info("There is no email address for this Local user ({0}), cannot send notification", localUser.Username);
                                continue;
                            }

                            Log.Info("Sending notification to: {0} at: {1}, for : {2}", localUser, localUser.EmailAddress, model.Title);
                            await PublishUserNotification(localUser.Username, localUser.EmailAddress, model.Title, model.PosterPath, type, model.Type);

                        }
                        else
                        {
                            var email = plexUser.User.FirstOrDefault(x => x.Username.Equals(user, StringComparison.CurrentCultureIgnoreCase));
                            if (string.IsNullOrEmpty(email?.Email))
                            {
                                Log.Info("There is no email address for this Plex user ({0}), cannot send notification", email?.Username);
                                // We do not have a plex user that requested this!
                                continue;
                            }

                            Log.Info("Sending notification to: {0} at: {1}, for : {2}", email.Username, email.Email, model.Title);
                            await PublishUserNotification(email.Username, email.Email, model.Title, model.PosterPath, type, model.Type);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public async Task NotifyUsers(RequestedModel model, NotificationType type)
        {
            try
            {
                var settings = await PlexSettings.GetSettingsAsync();
                var plexUser = PlexApi.GetUsers(settings.PlexAuthToken); // TODO emby
                var userAccount = PlexApi.GetAccount(settings.PlexAuthToken);
                var localUsers = UserHelper.GetUsers().ToList();

                var adminUsername = userAccount.Username ?? string.Empty;

                var users = UserHelper.GetUsersWithFeature(Features.RequestAddedNotification).ToList();
                Log.Debug("Notifying Users Count {0}", users.Count);

                // Get the usernames or alias depending if they have an alias
                var userNamesWithFeature = users.Select(x => x.UsernameOrAlias).ToList();
                Log.Debug("Users with the feature count {0}", userNamesWithFeature.Count);
                Log.Debug("Usernames: ");
                foreach (var u in userNamesWithFeature)
                {
                    Log.Debug(u);
                }
                
                Log.Debug("Users in the requested model count: {0}", model.AllUsers.Count);
                Log.Debug("usernames from model: ");
                foreach (var modelAllUser in model.AllUsers)
                {
                    Log.Debug(modelAllUser);
                }

                if (model.AllUsers == null || !model.AllUsers.Any())
                {
                    Log.Debug("There are no users in the model.AllUsers, no users to notify");
                    return;
                }
                var usersToNotify = userNamesWithFeature.Intersect(model.AllUsers, StringComparer.CurrentCultureIgnoreCase).ToList();

                if (!usersToNotify.Any())
                {
                    Log.Debug("Could not find any users after the .Intersect()");
                }

                Log.Debug("Users being notified for this request count {0}", users.Count);
                foreach (var user in usersToNotify)
                {
                    Log.Info("Notifying user {0}", user);
                    if (user.Equals(adminUsername, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Log.Info("This user is the Plex server owner");
                        await PublishUserNotification(userAccount.Username, userAccount.Email, model.Title, model.PosterPath, type, model.Type);
                        return;
                    }

                    var email = plexUser.User.FirstOrDefault(x => x.Username.Equals(user, StringComparison.CurrentCultureIgnoreCase));
                    if (email == null) // This is not a Plex User
                    {
                        // Local User?
                        var local = localUsers.FirstOrDefault(x => x.UsernameOrAlias.Equals(user));
                        if (local != null)
                        {

                            Log.Info("Sending notification to: {0} at: {1}, for title: {2}", local.UsernameOrAlias, local.EmailAddress, model.Title);
                            await PublishUserNotification(local.UsernameOrAlias, local.EmailAddress, model.Title, model.PosterPath, type, model.Type);
                            continue;
                        }
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