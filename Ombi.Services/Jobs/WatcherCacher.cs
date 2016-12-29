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
    public class WatcherCacher : IJob, IWatcherCacher
    {
        public WatcherCacher(
            ISettingsService<WatcherSettings> watcher,
            IWatcherApi watcherApi, ICacheProvider cache, IJobRecord rec)
        {
            WatcherSettings = watcher;
            WatcherApi = WatcherApi;
            Cache = cache;
            Job = rec;
        }
        
        private ISettingsService<WatcherSettings> WatcherSettings { get; }
        private ICacheProvider Cache { get; }
        private IWatcherApi WatcherApi { get; }
        private IJobRecord Job { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        public void Queued()
        {
            Log.Trace("Getting the settings");

            var watcherSettings = WatcherSettings.GetSettings();

            Job.SetRunning(true, JobNames.WatcherCacher);
            try
            {
                if (watcherSettings.Enabled)
                {
                    var movies = WatcherApi.ListMovies(watcherSettings.ApiKey, watcherSettings.FullUri);
                    if (movies.Error)
                    {
                        Log.Error("Error when trying to get Watchers movies");
                        Log.Error(movies.ErrorMessage);
                    }
                    var wantedMovies =
                        movies?.Results?.Where(x => x.status.Equals("Wanted", StringComparison.CurrentCultureIgnoreCase));
                    if (wantedMovies != null && wantedMovies.Any())
                    {
                        Cache.Set(CacheKeys.WatcherQueued, movies.Results.Select(x => x.imdbid), CacheKeys.TimeFrameMinutes.SchedulerCaching);
                    }

                }

            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                Job.Record(JobNames.WatcherCacher);
                Job.SetRunning(false, JobNames.WatcherCacher);
            }
        }

        // we do not want to set here...
        public string[] QueuedIds()
        {
            try
            {

                var watcherSettings = WatcherSettings.GetSettings();

                if (watcherSettings.Enabled)
                {
                    var movies = Cache.Get<string[]>(CacheKeys.WatcherQueued);

                    if (movies != null)
                    {
                        return movies;
                    }
                    return new string[] {};
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new string[] { };
            }
            return new string[] {};
        }

        public void Execute(IJobExecutionContext context)
        {
            Queued();
        }
    }
}
