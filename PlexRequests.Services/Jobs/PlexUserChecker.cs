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
using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Helpers.Permissions;
using PlexRequests.Services.Interfaces;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;

using Quartz;

namespace PlexRequests.Services.Jobs
{
    public class PlexUserChecker : IJob
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public PlexUserChecker(IPlexUserRepository plexUsers, IPlexApi plexAPi, IJobRecord rec, ISettingsService<PlexSettings> plexSettings, ISettingsService<PlexRequestSettings> prSettings)
        {
            Repo = plexUsers;
            JobRecord = rec;
            PlexApi = plexAPi;
            PlexSettings = plexSettings;
            PlexRequestSettings = prSettings;
        }

        private IJobRecord JobRecord { get; }
        private IPlexApi PlexApi { get; }
        private IPlexUserRepository Repo { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<PlexRequestSettings> PlexRequestSettings { get; }


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
                var prSettings = PlexRequestSettings.GetSettings();

                var dbUsers = Repo.GetAll().ToList();
                foreach (var user in plexUsers.User)
                {
                    var dbUser = dbUsers.FirstOrDefault(x => x.PlexUserId == user.Id);
                    if (dbUser != null)
                    {
                        var needToUpdate = false;

                        // Do we need up update any info?
                        if (dbUser.EmailAddress != user.Email)
                        {
                            dbUser.EmailAddress = user.Email;
                            needToUpdate = true;
                        }
                        if (dbUser.Username != user.Username)
                        {
                            dbUser.Username = user.Username;
                            needToUpdate = true;
                        }

                        if (needToUpdate)
                        {
                            Repo.Update(dbUser);
                        }

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