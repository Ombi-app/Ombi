#region Copyright

// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: StoreCleanup.cs
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
using System.Linq;
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Core.Users;
using Ombi.Services.Interfaces;
using Ombi.Store.Models;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Services.Jobs
{
    public class PlexUserChecker : IJob
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public PlexUserChecker(IPlexUserRepository plexUsers, IPlexApi plexAPi, IJobRecord rec, ISettingsService<PlexSettings> plexSettings, ISettingsService<PlexRequestSettings> prSettings, ISettingsService<UserManagementSettings> umSettings,
            IRequestService requestService, IUserRepository localUser)
        {
            Repo = plexUsers;
            JobRecord = rec;
            PlexApi = plexAPi;
            PlexSettings = plexSettings;
            PlexRequestSettings = prSettings;
            UserManagementSettings = umSettings;
            RequestService = requestService;
            LocalUserRepository = localUser;
        }

        private IJobRecord JobRecord { get; }
        private IPlexApi PlexApi { get; }
        private IPlexUserRepository Repo { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<PlexRequestSettings> PlexRequestSettings { get; }
        private ISettingsService<UserManagementSettings> UserManagementSettings { get; }
        private IRequestService RequestService { get; }
        private IUserRepository LocalUserRepository { get; }

        public void Execute(IJobExecutionContext context)
        {
            JobRecord.SetRunning(true, JobNames.PlexUserChecker);

            try
            {
                var settings = PlexSettings.GetSettings();
                if (string.IsNullOrEmpty(settings.PlexAuthToken))
                {
                    return;
                }
                var plexUsers = PlexApi.GetUsers(settings.PlexAuthToken);
                var userManagementSettings = UserManagementSettings.GetSettings();
                var requests = RequestService.GetAll().ToList();

                var dbUsers = Repo.GetAll().ToList();
                var localUsers = LocalUserRepository.GetAll().ToList();
                foreach (var user in plexUsers.User)
                {
                    var dbUser = dbUsers.FirstOrDefault(x => x.PlexUserId == user.Id);
                    if (dbUser != null)
                    {
                        // We already have the user, let's check if they have updated any of their info.
                        var needToUpdate = false;
                        var usernameChanged = false;

                        // Do we need up update any info?
                        if (!dbUser.EmailAddress.Equals(user.Email, StringComparison.CurrentCultureIgnoreCase))
                        {
                            dbUser.EmailAddress = user.Email;
                            needToUpdate = true;
                        }
                        if (!dbUser.Username.Equals(user.Username, StringComparison.CurrentCultureIgnoreCase))
                        {
                            needToUpdate = true;
                            usernameChanged = true;
                        }

                        if (needToUpdate)
                        {
                            if (usernameChanged)
                            {
                                // The username has changed, let's check if the username matches any local users
                                var localUser = localUsers.FirstOrDefault(x => x.UserName.Equals(user.Username, StringComparison.CurrentCultureIgnoreCase));
                                dbUser.Username = user.Username;
                                if (localUser != null)
                                {
                                    // looks like we have a local user with the same name...
                                    // We should delete the local user and the Plex user will become the master,
                                    // I am not going to update the Plex Users permissions as that could end up leading to a security vulnerability
                                    // Where anyone could change their Plex Username to the PR.Net server admins name and get all the admin permissions.

                                    LocalUserRepository.Delete(localUser);
                                }

                                // Since the username has changed, we need to update all requests with that username (unless we are using the alias! Since the alias won't change)
                                if (string.IsNullOrEmpty(dbUser.UserAlias))
                                {
                                    // Update all requests
                                    var requestsWithThisUser = requests.Where(x => x.RequestedUsers.Contains(user.Username)).ToList();
                                    foreach (var r in requestsWithThisUser)
                                    {
                                        r.RequestedUsers.Remove(user.Username); // Remove old
                                        r.RequestedUsers.Add(dbUser.Username); // Add new
                                    }

                                    if (requestsWithThisUser.Any())
                                    {
                                        RequestService.BatchUpdate(requestsWithThisUser);
                                    }
                                }
                            }
                            Repo.Update(dbUser);
                        }

                        continue;
                    }
                    
                    // Looks like it's a new user!
                   var m = new PlexUsers
                    {
                        PlexUserId = user.Id,
                        Permissions = UserManagementHelper.GetPermissions(userManagementSettings),
                        Features = UserManagementHelper.GetFeatures(userManagementSettings),
                        UserAlias = string.Empty,
                        EmailAddress = user.Email,
                        Username = user.Username,
                        LoginId = Guid.NewGuid().ToString()
                    };

                    Repo.Insert(m);
                }

            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                JobRecord.SetRunning(false, JobNames.PlexUserChecker);
                JobRecord.Record(JobNames.PlexUserChecker);
            }
        }
    }
}