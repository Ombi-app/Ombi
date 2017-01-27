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

using System.Collections.Generic;
using System.Linq;
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Radarr;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Services.Interfaces;
using Quartz;

namespace Ombi.Services.Jobs
{
    public class RadarrCacher : IJob, IRadarrCacher
    {
        public RadarrCacher(ISettingsService<RadarrSettings> radarrService, IRadarrApi radarrApi, ICacheProvider cache, IJobRecord rec)
        {
            RadarrSettings = radarrService;
            RadarrApi = radarrApi;
            Job = rec;
            Cache = cache;
        }

        private ISettingsService<RadarrSettings> RadarrSettings { get; }
        private ICacheProvider Cache { get; }
        private IRadarrApi RadarrApi { get; }
        private IJobRecord Job { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        public void Queued()
        {
            var settings = RadarrSettings.GetSettings();
            if (settings.Enabled)
            {
                Job.SetRunning(true, JobNames.RadarrCacher);
                try
                {
                    var movies = RadarrApi.GetMovies(settings.ApiKey, settings.FullUri);
                    if (movies != null)
                    {
                        var movieIds = new List<int>();
                        foreach (var m in movies)
                        {
                            if (m.tmdbId > 0)
                            {
                                movieIds.Add(m.tmdbId);
                            }
                        }
                        //var movieIds = movies.Select(x => x.tmdbId).ToList();
                        Cache.Set(CacheKeys.RadarrMovies, movieIds, CacheKeys.TimeFrameMinutes.SchedulerCaching);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex, "Failed caching queued items from Radarr");
                }
                finally
                {
                    Job.Record(JobNames.RadarrCacher);
                    Job.SetRunning(false, JobNames.RadarrCacher);
                }
            }
        }

        // we do not want to set here...
        public int[] QueuedIds()
        {
            var retVal = new List<int>();
            var movies = Cache.Get<List<int>>(CacheKeys.RadarrMovies);
            if (movies != null)
            {
                retVal.AddRange(movies);
            }
            return retVal.ToArray();
        }

        public void Execute(IJobExecutionContext context)
        {
            Queued();
        }
    }
}