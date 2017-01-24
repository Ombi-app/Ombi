#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: AboutModule.cs
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
using Nancy;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers.Permissions;
using Ombi.Services.Interfaces;
using Ombi.Services.Jobs;
using Ombi.UI.Models;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Modules.Admin
{
    public class ScheduledJobsRunnerModule : BaseModule
    {
        public ScheduledJobsRunnerModule(ISettingsService<PlexRequestSettings> settingsService,
            ISecurityExtensions security, IPlexContentCacher contentCacher, ISonarrCacher sonarrCacher, IWatcherCacher watcherCacher,
            IRadarrCacher radarrCacher, ICouchPotatoCacher cpCacher, IStoreBackup store, ISickRageCacher srCacher, IAvailabilityChecker plexChceker,
            IStoreCleanup cleanup, IUserRequestLimitResetter requestLimit, IPlexEpisodeCacher episodeCacher, IRecentlyAdded recentlyAdded,
            IFaultQueueHandler faultQueueHandler, IPlexUserChecker plexUserChecker) : base("admin", settingsService, security)
        {
            Before += (ctx) => Security.AdminLoginRedirect(Permissions.Administrator, ctx);

            PlexContentCacher = contentCacher;
            SonarrCacher = sonarrCacher;
            RadarrCacher = radarrCacher;
            WatcherCacher = watcherCacher;
            CpCacher = cpCacher;
            StoreBackup = store;
            SrCacher = srCacher;
            AvailabilityChecker = plexChceker;
            StoreCleanup = cleanup;
            RequestLimit = requestLimit;
            EpisodeCacher = episodeCacher;
            RecentlyAdded = recentlyAdded;
            FaultQueueHandler = faultQueueHandler;
            PlexUserChecker = plexUserChecker;

            Post["/schedulerun", true] = async (x, ct) => await ScheduleRun((string)Request.Form.key);
        }

        private IPlexContentCacher PlexContentCacher { get; }
        private IRadarrCacher RadarrCacher { get; }
        private ISonarrCacher SonarrCacher { get; }
        private IWatcherCacher WatcherCacher { get; }
        private ICouchPotatoCacher CpCacher { get; }
        private IStoreBackup StoreBackup { get; }
        private ISickRageCacher SrCacher { get; }
        private IAvailabilityChecker AvailabilityChecker { get; }
        private IStoreCleanup StoreCleanup { get; }
        private IUserRequestLimitResetter RequestLimit { get; }
        private IPlexEpisodeCacher EpisodeCacher { get; }
        private IRecentlyAdded RecentlyAdded { get; }
        private IFaultQueueHandler FaultQueueHandler { get; }
        private IPlexUserChecker PlexUserChecker { get; }


        private async Task<Response> ScheduleRun(string key)
        {
            if (key.Equals(JobNames.PlexCacher, StringComparison.CurrentCultureIgnoreCase))
            {
                PlexContentCacher.CacheContent();
            }

            if (key.Equals(JobNames.WatcherCacher, StringComparison.CurrentCultureIgnoreCase))
            {
                WatcherCacher.Queued();
            }

            if (key.Equals(JobNames.SonarrCacher, StringComparison.CurrentCultureIgnoreCase))
            {
                SonarrCacher.Queued();
            }
            if (key.Equals(JobNames.RadarrCacher, StringComparison.CurrentCultureIgnoreCase))
            {
                RadarrCacher.Queued();
            }
            if (key.Equals(JobNames.CpCacher, StringComparison.CurrentCultureIgnoreCase))
            {
                CpCacher.Queued();
            }
            if (key.Equals(JobNames.StoreBackup, StringComparison.CurrentCultureIgnoreCase))
            {
                StoreBackup.Start();
            }
            if (key.Equals(JobNames.SrCacher, StringComparison.CurrentCultureIgnoreCase))
            {
                SrCacher.Queued();
            }
            if (key.Equals(JobNames.PlexChecker, StringComparison.CurrentCultureIgnoreCase))
            {
                AvailabilityChecker.Start();
            }
            if (key.Equals(JobNames.StoreCleanup, StringComparison.CurrentCultureIgnoreCase))
            {
                StoreCleanup.Start();
            }
            if (key.Equals(JobNames.RequestLimitReset, StringComparison.CurrentCultureIgnoreCase))
            {
                RequestLimit.Start();
            }
            if (key.Equals(JobNames.EpisodeCacher, StringComparison.CurrentCultureIgnoreCase))
            {
                EpisodeCacher.Start();
            }
            if (key.Equals(JobNames.RecentlyAddedEmail, StringComparison.CurrentCultureIgnoreCase))
            {
                RecentlyAdded.Start();
            }
            if (key.Equals(JobNames.FaultQueueHandler, StringComparison.CurrentCultureIgnoreCase))
            {
                FaultQueueHandler.Start();
            }
            if (key.Equals(JobNames.PlexUserChecker, StringComparison.CurrentCultureIgnoreCase))
            {
                RequestLimit.Start();
            }


            return Response.AsJson(new JsonResponseModel { Result = true });
        }
    }
}