#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: Version1100.cs
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
using System.Data;
using NLog;
using System.Linq;
using PlexRequests.Api.Interfaces;
using PlexRequests.Core.SettingModels;
using PlexRequests.Core.Users;
using PlexRequests.Helpers;
using PlexRequests.Helpers.Permissions;
using PlexRequests.Store;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;

namespace PlexRequests.Core.Migration.Migrations
{
    [Migration(11000, "v1.10.0.0")]
    public class Version1100 : BaseMigration, IMigration
    {
        public Version1100(IUserRepository userRepo, IRequestService requestService, ISettingsService<LogSettings> log, IPlexApi plexApi, ISettingsService<PlexSettings> plexService,
            IPlexUserRepository plexusers, ISettingsService<PlexRequestSettings> prSettings, ISettingsService<UserManagementSettings> umSettings,
            ISettingsService<ScheduledJobsSettings> sjs, IRepository<UsersToNotify> usersToNotify)
        {
            UserRepo = userRepo;
            RequestService = requestService;
            Log = log;
            PlexApi = plexApi;
            PlexSettings = plexService;
            PlexUsers = plexusers;
            PlexRequestSettings = prSettings;
            UserManagementSettings = umSettings;
            ScheduledJobSettings = sjs;
            UserNotifyRepo = usersToNotify;
        }
        public int Version => 11000;
        private IUserRepository UserRepo { get; }
        private IRequestService RequestService { get; }
        private ISettingsService<LogSettings> Log { get; }
        private IPlexApi PlexApi { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private IPlexUserRepository PlexUsers { get; }
        private ISettingsService<PlexRequestSettings> PlexRequestSettings { get; }
        private ISettingsService<UserManagementSettings> UserManagementSettings { get; }
        private ISettingsService<ScheduledJobsSettings> ScheduledJobSettings { get; }
        private IRepository<UsersToNotify> UserNotifyRepo { get; }

        public void Start(IDbConnection con)
        {
            UpdateDb(con);

            // Update the current admin permissions set

            PopulateDefaultUserManagementSettings();
            UpdateAdmin();
            ResetLogLevel();
            UpdatePlexUsers();
            UpdateScheduledJobs();
            MigrateUserNotifications();

            UpdateSchema(con, Version);
        }

        private void MigrateUserNotifications()
        {
            var usersToNotify = UserNotifyRepo.GetAll();
            var plexUsers = PlexUsers.GetAll().ToList();
            var users = UserRepo.GetAll().ToList();
            foreach (var u in usersToNotify)
            {
                var selectedPlexUser = plexUsers.FirstOrDefault(x => x.Username.Equals(u.Username, StringComparison.CurrentCultureIgnoreCase));
                if (selectedPlexUser != null)
                {
                    selectedPlexUser.Features += (int)Features.RequestAddedNotification;
                    PlexUsers.Update(selectedPlexUser);
                }

                var selectedLocalUser =
                    users.FirstOrDefault(x => x.UserName.Equals(u.Username, StringComparison.CurrentCultureIgnoreCase));
                if (selectedLocalUser != null)
                {
                    selectedLocalUser.Features += (int)Features.RequestAddedNotification;
                    UserRepo.Update(selectedLocalUser);
                }

            }
        }

        private void UpdateScheduledJobs()
        {
            var settings = ScheduledJobSettings.GetSettings();

            settings.PlexUserChecker = 24;
            settings.PlexContentCacher = 60;

            ScheduledJobSettings.SaveSettings(settings);
        }

        private void PopulateDefaultUserManagementSettings()
        {
            var plexRequestSettings = PlexRequestSettings.GetSettings();

            UserManagementSettings.SaveSettings(new UserManagementSettings
            {
                AutoApproveMovies = !plexRequestSettings.RequireMovieApproval,
                RequestTvShows = plexRequestSettings.SearchForTvShows,
                RequestMusic = plexRequestSettings.SearchForMusic,
                RequestMovies = plexRequestSettings.SearchForMovies,
                AutoApproveMusic = !plexRequestSettings.RequireMusicApproval,
                AutoApproveTvShows = !plexRequestSettings.RequireTvShowApproval
            });
        }

        private void UpdatePlexUsers()
        {
            var settings = PlexSettings.GetSettings();
            if (string.IsNullOrEmpty(settings.PlexAuthToken))
            {
                return;
            }
            var plexUsers = PlexApi.GetUsers(settings.PlexAuthToken);
            var prSettings = PlexRequestSettings.GetSettings();

            var dbUsers = PlexUsers.GetAll().ToList();
            foreach (var user in plexUsers.User)
            {
                if (dbUsers.FirstOrDefault(x => x.PlexUserId == user.Id) != null)
                {
                    continue;
                }

                int permissions = 0;
                if (prSettings.SearchForMovies)
                {
                    permissions = (int)Permissions.RequestMovie;
                }
                if (prSettings.SearchForTvShows)
                {
                    permissions += (int)Permissions.RequestTvShow;
                }
                if (prSettings.SearchForMusic)
                {
                    permissions += (int)Permissions.RequestMusic;
                }
                if (!prSettings.RequireMovieApproval)
                {
                    permissions += (int)Permissions.AutoApproveMovie;
                }
                if (!prSettings.RequireTvShowApproval)
                {
                    permissions += (int)Permissions.AutoApproveTv;
                }
                if (!prSettings.RequireMusicApproval)
                {
                    permissions += (int)Permissions.AutoApproveAlbum;
                }

                // Add report Issues

                permissions += (int)Permissions.ReportIssue;

                var m = new PlexUsers
                {
                    PlexUserId = user.Id,
                    Permissions = permissions,
                    Features = 0,
                    UserAlias = string.Empty,
                    EmailAddress = user.Email,
                    Username = user.Username,
                    LoginId = Guid.NewGuid().ToString()
                };

                PlexUsers.Insert(m);
            }

        }

        private void ResetLogLevel()
        {
            var logSettings = Log.GetSettings();
            logSettings.Level = LogLevel.Error.Ordinal;
            Log.SaveSettings(logSettings);

            LoggingHelper.ReconfigureLogLevel(LogLevel.FromOrdinal(logSettings.Level));
        }

        private void UpdateDb(IDbConnection con)
        {
            // Create the two new columns
            con.AlterTable("Users", "ADD", "Permissions", true, "INTEGER");
            con.AlterTable("Users", "ADD", "Features", true, "INTEGER");

            con.AlterTable("PlexUsers", "ADD", "Permissions", true, "INTEGER");
            con.AlterTable("PlexUsers", "ADD", "Features", true, "INTEGER");
            con.AlterTable("PlexUsers", "ADD", "Username", true, "VARCHAR(100)");
            con.AlterTable("PlexUsers", "ADD", "EmailAddress", true, "VARCHAR(100)");
            con.AlterTable("PlexUsers", "ADD", "LoginId", true, "VARCHAR(100)");

            //https://image.tmdb.org/t/p/w150/https://image.tmdb.org/t/p/w150//aqhAqttDq7zgsTaBHtCD8wmTk6k.jpg 

            // UI = https://image.tmdb.org/t/p/w150/{{posterPath}}
            // Update old invalid posters
            var allRequests = RequestService.GetAll().ToList();

            foreach (var req in allRequests)
            {
                if (req.PosterPath.Contains("https://image.tmdb.org/t/p/w150/"))
                {
                    var newImg = req.PosterPath.Replace("https://image.tmdb.org/t/p/w150/", string.Empty);
                    req.PosterPath = newImg;
                }
            }
            RequestService.BatchUpdate(allRequests);
        }

        private void UpdateAdmin()
        {
            var users = UserRepo.GetAll().ToList();

            foreach (var user in users)
            {
                user.Permissions = (int)
                    (Permissions.Administrator
                    | Permissions.ReportIssue
                    | Permissions.RequestMusic
                    | Permissions.RequestTvShow
                    | Permissions.RequestMovie
                    | Permissions.AutoApproveAlbum
                    | Permissions.AutoApproveMovie
                    | Permissions.AutoApproveTv);
            }

            UserRepo.UpdateAll(users);
        }
    }
}
