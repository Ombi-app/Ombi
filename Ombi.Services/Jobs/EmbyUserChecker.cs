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
using Ombi.Helpers.Permissions;
using Ombi.Services.Interfaces;
using Ombi.Store.Models.Emby;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Services.Jobs
{
    public class EmbyUserChecker : IJob, IEmbyUserChecker
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public EmbyUserChecker(IExternalUserRepository<EmbyUsers> plexUsers, IEmbyApi embyApi, IJobRecord rec, ISettingsService<EmbySettings> embyS, ISettingsService<PlexRequestSettings> prSettings, ISettingsService<UserManagementSettings> umSettings,
            IRequestService requestService, IUserRepository localUser)
        {
            Repo = plexUsers;
            JobRecord = rec;
            EmbyApi = embyApi;
            EmbySettings = embyS;
            PlexRequestSettings = prSettings;
            UserManagementSettings = umSettings;
            RequestService = requestService;
            LocalUserRepository = localUser;
        }

        private IJobRecord JobRecord { get; }
        private IEmbyApi EmbyApi { get; }
        private IExternalUserRepository<EmbyUsers> Repo { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
        private ISettingsService<PlexRequestSettings> PlexRequestSettings { get; }
        private ISettingsService<UserManagementSettings> UserManagementSettings { get; }
        private IRequestService RequestService { get; }
        private IUserRepository LocalUserRepository { get; }

        public void Start()
        {
            JobRecord.SetRunning(true, JobNames.EmbyUserChecker);

            try
            {
                var settings = EmbySettings.GetSettings();
                if (string.IsNullOrEmpty(settings.ApiKey) || !settings.Enable)
                {
                    return;
                }
                var embyUsers = EmbyApi.GetUsers(settings.FullUri, settings.ApiKey);
                var userManagementSettings = UserManagementSettings.GetSettings();
                var requests = RequestService.GetAll().ToList();

                var dbUsers = Repo.GetAll().ToList();
                var localUsers = LocalUserRepository.GetAll().ToList();

                // Regular users
                foreach (var user in embyUsers)
                {
                    var dbUser = dbUsers.FirstOrDefault(x => x.EmbyUserId == user.Id);
                    if (dbUser != null)
                    {
                        // we already have a user
                        continue;
                    }

                    // Looks like it's a new user!
                    var m = new EmbyUsers
                    {
                        EmbyUserId = user.Id,
                        Permissions = UserManagementHelper.GetPermissions(userManagementSettings),
                        Features = UserManagementHelper.GetFeatures(userManagementSettings),
                        UserAlias = string.Empty,
                        Username = user.Name,
                        LoginId = Guid.NewGuid().ToString()
                    };


                    // If it's the admin, give them the admin permission
                    if (user.Policy.IsAdministrator)
                    {
                        if (!((Permissions) m.Permissions).HasFlag(Permissions.Administrator))
                        {
                            m.Permissions += (int)Permissions.Administrator;
                        }
                    }

                    Repo.Insert(m);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                JobRecord.SetRunning(false, JobNames.EmbyUserChecker);
                JobRecord.Record(JobNames.EmbyUserChecker);
            }
        }
        public void Execute(IJobExecutionContext context)
        {
            Start();
        }
    }
}