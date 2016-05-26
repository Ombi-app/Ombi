#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexAvailabilityChecker.cs
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
using System.Linq;

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.SickRage;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services.Interfaces;

using Quartz;

namespace PlexRequests.Services.Jobs
{
    public class SickRageCacher : IJob, ISickRageCacher
    {
        public SickRageCacher(ISettingsService<SickRageSettings> srSettings, ISickRageApi srApi, ICacheProvider cache, IJobRecord rec)
        {
            SrSettings = srSettings;
            SrApi = srApi;
            Cache = cache;
            Job = rec;
        }

        private ISettingsService<SickRageSettings> SrSettings { get; }
        private ICacheProvider Cache { get; }
        private ISickRageApi SrApi { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();
        private IJobRecord Job { get; }

        public void Queued()
        {
            Log.Trace("Getting the settings");

            var settings = SrSettings.GetSettings();
            if (settings.Enabled)
            {
                Log.Trace("Getting all shows from SickRage");
                try
                {
                    var shows = SrApi.GetShows(settings.ApiKey, settings.FullUri);
                    if (shows != null)
                    {
                        Cache.Set(CacheKeys.SickRageQueued, shows.Result, CacheKeys.TimeFrameMinutes.SchedulerCaching);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex, "Failed caching queued items from SickRage");
                }
                finally
                {
                    Job.Record(JobNames.SrCacher);
                }
            }
        }

        // we do not want to set here...
        public int[] QueuedIds()
        {
            var tv = Cache.Get<SickrageShows>(CacheKeys.SickRageQueued);
            return tv?.data?.Values.Select(x => x.tvdbid).ToArray() ?? new int[] { };
        }

        public void Execute(IJobExecutionContext context)
        {
            Queued();
        }
    }
}