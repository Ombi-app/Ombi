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
using Ombi.Store.Models.Emby;
using Ombi.Store.Repository;

namespace Ombi.Services.Notification
{
    public class EmbyNotificationEngine : IEmbyNotificationEngine
    {
        public EmbyNotificationEngine(IEmbyApi p, IRepository<UsersToNotify> repo, ISettingsService<EmbySettings> embySettings, INotificationService service, IUserHelper userHelper, IExternalUserRepository<EmbyUsers> embyUsers)
        {
            EmbyApi = p;
            UserNotifyRepo = repo;
            Notification = service;
            UserHelper = userHelper;
            EmbySettings = embySettings;
            EmbyUserRepo = embyUsers;
        }

        private IEmbyApi EmbyApi { get; }
        private IRepository<UsersToNotify> UserNotifyRepo { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private INotificationService Notification { get; }
        private IUserHelper UserHelper { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
        private IExternalUserRepository<EmbyUsers> EmbyUserRepo { get; }

        public async Task NotifyUsers(IEnumerable<RequestedModel> modelChanged, NotificationType type)
        {
            try
            {
                var embySettings = await EmbySettings.GetSettingsAsync();
                var embyUsers = EmbyApi.GetUsers(embySettings.FullUri, embySettings.ApiKey);
                var userAccount = embyUsers.FirstOrDefault(x => x.Policy.IsAdministrator);

                var adminUsername = userAccount?.Name ?? string.Empty;
                
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
                        var localUser =
                             users.FirstOrDefault(x =>
                                    x.Username.Equals(user, StringComparison.CurrentCultureIgnoreCase) ||
                                    x.UserAlias.Equals(user, StringComparison.CurrentCultureIgnoreCase));
                        Log.Info("Notifying user {0}", user);
                        if (user.Equals(adminUsername, StringComparison.CurrentCultureIgnoreCase))
                        {
                            Log.Info("This user is the Plex server owner");
                            await PublishUserNotification(userAccount?.Name, localUser?.EmailAddress, model.Title, model.PosterPath, type, model.Type); 
                            return;
                        }



                        // So if the request was from an alias, then we need to use the local user (since that contains the alias).
                        // If we do not have a local user, then we should be using the Emby user if that user exists.
                        // This will execute most of the time since Emby and Local users will most always be in the database.
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
                            var embyUser = EmbyUserRepo.GetUserByUsername(user);
                            var email = embyUsers.FirstOrDefault(x => x.Name.Equals(user, StringComparison.CurrentCultureIgnoreCase));
                            if (string.IsNullOrEmpty(embyUser?.EmailAddress)) // TODO this needs to be the email
                            {
                                Log.Info("There is no email address for this Emby user ({0}), cannot send notification", email?.Name); 
                                // We do not have a plex user that requested this!
                                continue;
                            }

                            Log.Info("Sending notification to: {0} at: {1}, for : {2}", embyUser?.Username, embyUser?.EmailAddress, model.Title);
                            await PublishUserNotification(email?.Name, embyUser?.EmailAddress, model.Title, model.PosterPath, type, model.Type);
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
                var embySettings = await EmbySettings.GetSettingsAsync();
                var embyUsers = EmbyApi.GetUsers(embySettings.FullUri, embySettings.ApiKey);
                var userAccount = embyUsers.FirstOrDefault(x => x.Policy.IsAdministrator);

                var adminUsername = userAccount.Name ?? string.Empty;

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
                    var embyUser = EmbyUserRepo.GetUserByUsername(user);
                    Log.Info("Notifying user {0}", user);
                    if (user.Equals(adminUsername, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Log.Info("This user is the Emby server owner");
                        await PublishUserNotification(userAccount.Name, embyUser.EmailAddress, model.Title, model.PosterPath, type, model.Type);
                        return;
                    }

                    var email = embyUsers.FirstOrDefault(x => x.Name.Equals(user, StringComparison.CurrentCultureIgnoreCase));
                    if (email == null)
                    {
                        Log.Info("There is no email address for this Emby user, cannot send notification");
                        // We do not have a emby user that requested this!
                        continue;
                    }

                    Log.Info("Sending notification to: {0} at: {1}, for title: {2}", email.Name, embyUser.EmailAddress, model.Title); 
                    await PublishUserNotification(email.Name, embyUser.EmailAddress, model.Title, model.PosterPath, type, model.Type); 
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