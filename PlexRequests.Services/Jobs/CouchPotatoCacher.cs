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

using System;
using System.Linq;
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Movie;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Services.Interfaces;
using Quartz;

namespace Ombi.Services.Jobs
{
    public class CouchPotatoCacher : IJob, ICouchPotatoCacher
    {
        public CouchPotatoCacher(ISettingsService<CouchPotatoSettings> cpSettings, ICouchPotatoApi cpApi, ICacheProvider cache, IJobRecord rec)
        {
            CpSettings = cpSettings;
            CpApi = cpApi;
            Cache = cache;
            Job = rec;
        }

        private ISettingsService<CouchPotatoSettings> CpSettings { get; }
        private ICacheProvider Cache { get; }
        private ICouchPotatoApi CpApi { get; }
        private IJobRecord Job { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        public void Queued()
        {
            Log.Trace("Getting the settings");

            var settings = CpSettings.GetSettings();
            if (settings.Enabled)
            {
                Job.SetRunning(true, JobNames.CpCacher);
                Log.Trace("Getting all movies from CouchPotato");
                try
                {
                    var movies = CpApi.GetMovies(settings.FullUri, settings.ApiKey, new[] { "active" });
                    if (movies != null)
                    {
                        Cache.Set(CacheKeys.CouchPotatoQueued, movies, CacheKeys.TimeFrameMinutes.SchedulerCaching);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex, "Failed caching queued items from CouchPotato");
                }
                finally
                {
                    Job.Record(JobNames.CpCacher);
                    Job.SetRunning(false, JobNames.CpCacher);
                }
            }
        }

        // we do not want to set here...
        public int[] QueuedIds()
        {
            try
            {
                var movies = Cache.Get<CouchPotatoMovies>(CacheKeys.CouchPotatoQueued);

                var items = movies?.movies?.Select(x => x.info?.tmdb_id);
                if(items != null)
                {
                    return items.Cast<int>().ToArray();
                }
                return new int[] { };
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new int[] {};
            }
        }

        public void Execute(IJobExecutionContext context)
        {
            Queued();
        }
    }
}
