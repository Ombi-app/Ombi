﻿#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: LayoutModule.cs
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
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses;
using NLog;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Core.StatusChecker;
using Ombi.Core.Users;
using Ombi.Helpers;
using Ombi.Services.Interfaces;
using Ombi.Services.Jobs;
using Ombi.UI.Models;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Modules
{
    public class LayoutModule : BaseAuthModule
    {
        public LayoutModule(ICacheProvider provider, ISettingsService<PlexRequestSettings> pr, ISettingsService<SystemSettings> settings, IJobRecord rec, ISecurityExtensions security, IUserHelper helper) : base("layout", pr, security)
        {
            Cache = provider;
            SystemSettings = settings;
            Job = rec;
            UserHelper = helper;

            Get["/", true] = async (x,ct) => await CheckLatestVersion();
            Get["/cacher", true] = async (x,ct) => await CacherRunning();
            Get["/gravatar"] = x => GetGravatarImage();
        }

        private ICacheProvider Cache { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();
        private ISettingsService<SystemSettings> SystemSettings { get; }
        private IJobRecord Job { get; }
        private IUserHelper UserHelper { get; }

        private async Task<Response> CheckLatestVersion()
        {
            try
            {
                if (!IsAdmin)
                {
                    return Response.AsJson(new JsonUpdateAvailableModel { UpdateAvailable = false });
                }
#if DEBUG
                return Response.AsJson(new JsonUpdateAvailableModel { UpdateAvailable = false });
#endif
                var checker = new StatusChecker(SystemSettings);
                var release = await Cache.GetOrSetAsync(CacheKeys.LastestProductVersion, async() => await checker.GetStatus(), 30);

                return Response.AsJson(release.UpdateAvailable 
                    ? new JsonUpdateAvailableModel { UpdateAvailable = true} 
                    : new JsonUpdateAvailableModel { UpdateAvailable = false });
            }
            catch (Exception e)
            {
                Log.Warn("Exception Thrown when attempting to check the status");
                Log.Warn(e);
                return Response.AsJson(new JsonUpdateAvailableModel { UpdateAvailable = false });
            }
        }

        private async Task<Response> CacherRunning()
        {
            try
            {
                var jobs = await Job.GetJobsAsync();

                // Check to see if any are running
                var runningJobs = jobs.Where(x => x.Running);

                // We only want the cachers
                var cacherJobs = runningJobs.Where(x =>
                           x.Name.Equals(JobNames.CpCacher) 
                        || x.Name.Equals(JobNames.EpisodeCacher) 
                        || x.Name.Equals(JobNames.PlexChecker) 
                        || x.Name.Equals(JobNames.SonarrCacher)
                        || x.Name.Equals(JobNames.SrCacher)
                        || x.Name.Equals(JobNames.PlexCacher)
                        || x.Name.Equals(JobNames.WatcherCacher));


                return Response.AsJson(cacherJobs.Any() 
                    ? new { CurrentlyRunning = true, IsAdmin} 
                    : new { CurrentlyRunning = false, IsAdmin });
            }
            catch (Exception e)
            {
                Log.Warn("Exception Thrown when attempting to check the status");
                Log.Warn(e);
                return Response.AsJson(new { CurrentlyRunning = false, IsAdmin });
            }
        }

        private Response GetGravatarImage()
        {
            if (LoggedIn)
            {
                var user = UserHelper.GetUser(Username);
                var hashed = StringHasher.CalcuateMd5Hash(user.EmailAddress);
                if (string.IsNullOrEmpty(hashed))
                {
                    return Response.AsJson(new JsonResponseModel
                    {
                        Result = false
                    });
                }
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = true,
                        Message = $"https://www.gravatar.com/avatar/{hashed}"
                    });
            }
            else
            {
                return Response.AsJson(new JsonResponseModel {Result = false});
            }
        }
    }
}