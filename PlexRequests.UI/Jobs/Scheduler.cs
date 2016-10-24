#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: Scheduler.cs
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
using System.Linq;

using NLog;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Services.Jobs;
using PlexRequests.UI.Helpers;

using Quartz;
using Quartz.Impl;

namespace PlexRequests.UI.Jobs
{
    internal sealed class Scheduler : IJobScheduler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private IServiceLocator Service => ServiceLocator.Instance;

        private readonly ISchedulerFactory _factory;

        public Scheduler()
        {
            _factory = new StdSchedulerFactory();
        }

        private IEnumerable<IJobDetail> CreateJobs()
        {
            var settingsService = Service.Resolve<ISettingsService<ScheduledJobsSettings>>();
            var s = settingsService.GetSettings();

            var jobs = new List<IJobDetail>();

            var jobList = new List<IJobDetail>
            {
               JobBuilder.Create<PlexAvailabilityChecker>().WithIdentity("PlexAvailabilityChecker", "Plex").Build(),
               JobBuilder.Create<PlexEpisodeCacher>().WithIdentity("PlexEpisodeCacher", "Cache").Build(),
               JobBuilder.Create<SickRageCacher>().WithIdentity("SickRageCacher", "Cache").Build(),
               JobBuilder.Create<SonarrCacher>().WithIdentity("SonarrCacher", "Cache").Build(),
               JobBuilder.Create<CouchPotatoCacher>().WithIdentity("CouchPotatoCacher", "Cache").Build(),
               JobBuilder.Create<StoreBackup>().WithIdentity("StoreBackup", "Database").Build(),
               JobBuilder.Create<StoreCleanup>().WithIdentity("StoreCleanup", "Database").Build(),
               JobBuilder.Create<UserRequestLimitResetter>().WithIdentity("UserRequestLimiter", "Request").Build(),
            };

            if (!string.IsNullOrEmpty(s.RecentlyAddedCron))
            {
                jobList.Add(JobBuilder.Create<RecentlyAdded>().WithIdentity("RecentlyAddedModel", "Email").Build());
            }


            jobs.AddRange(jobList);

            return jobs;
        }

        /// <summary>
        /// Starts the scheduler.
        /// </summary>
        /// <exception cref="System.InvalidProgramException">The jobs do not match the triggers, we should have a trigger per job</exception>
        public void StartScheduler()
        {
            var scheduler = _factory.GetScheduler();
            scheduler.JobFactory = new CustomJobFactory();
            scheduler.Start();

            var jobs = CreateJobs();
            var triggers = CreateTriggers();

            var jobDetails = jobs as IJobDetail[] ?? jobs.ToArray();
            var triggerDetails = triggers as ITrigger[] ?? triggers.ToArray();

            if (jobDetails.Length != triggerDetails.Length)
            {
                Log.Error("The jobs do not match the triggers, we should have a trigger per job");
                throw new InvalidProgramException("The jobs do not match the triggers, we should have a trigger per job");
            }

            for (var i = 0; i < jobDetails.Length; i++)
            {
                scheduler.ScheduleJob(jobDetails[i], triggerDetails[i]);
            }
        }

        private IEnumerable<ITrigger> CreateTriggers()
        {
            var settingsService = Service.Resolve<ISettingsService<ScheduledJobsSettings>>();
            var s = settingsService.GetSettings();

            var triggers = new List<ITrigger>();

            var plexAvailabilityChecker =
                TriggerBuilder.Create()
                    .WithIdentity("PlexAvailabilityChecker", "Plex")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(s.PlexAvailabilityChecker).RepeatForever())
                    .Build();

            var srCacher =
                TriggerBuilder.Create()
                    .WithIdentity("SickRageCacher", "Cache")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(s.SickRageCacher).RepeatForever())
                    .Build();

            var sonarrCacher =
                TriggerBuilder.Create()
                    .WithIdentity("SonarrCacher", "Cache")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(s.SonarrCacher).RepeatForever())
                    .Build();

            var cpCacher =
                TriggerBuilder.Create()
                    .WithIdentity("CouchPotatoCacher", "Cache")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(s.CouchPotatoCacher).RepeatForever())
                    .Build();

            var storeBackup =
                TriggerBuilder.Create()
                    .WithIdentity("StoreBackup", "Database")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInHours(s.StoreBackup).RepeatForever())
                    .Build();

            var storeCleanup =
                TriggerBuilder.Create()
                    .WithIdentity("StoreCleanup", "Database")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInHours(s.StoreCleanup).RepeatForever())
                    .Build();

            var userRequestLimiter =
                TriggerBuilder.Create()
                    .WithIdentity("UserRequestLimiter", "Request")
                    .StartAt(DateBuilder.FutureDate(5, IntervalUnit.Minute))
                    // Everything has started on application start, lets wait 5 minutes
                    .WithSimpleSchedule(x => x.WithIntervalInHours(s.UserRequestLimitResetter).RepeatForever())
                    .Build();

            var plexEpCacher =
                TriggerBuilder.Create()
                    .WithIdentity("PlexEpisodeCacher", "Cache")
                    .StartAt(DateBuilder.FutureDate(5, IntervalUnit.Minute))
                    .WithSimpleSchedule(x => x.WithIntervalInHours(s.PlexEpisodeCacher).RepeatForever())
                    .Build();


            var cronJob = string.IsNullOrEmpty(s.RecentlyAddedCron);
            if (!cronJob)
            {
                var rencentlyAdded =
                    TriggerBuilder.Create()
                        .WithIdentity("RecentlyAddedModel", "Email")
                        .StartNow()
                        .WithCronSchedule(s.RecentlyAddedCron)
                        .WithSimpleSchedule(x => x.WithIntervalInHours(2).RepeatForever())
                        .Build();

                triggers.Add(rencentlyAdded);
            }



            triggers.Add(plexAvailabilityChecker);
            triggers.Add(srCacher);
            triggers.Add(sonarrCacher);
            triggers.Add(cpCacher);
            triggers.Add(storeBackup);
            triggers.Add(storeCleanup);
            triggers.Add(userRequestLimiter);
            triggers.Add(plexEpCacher);

            return triggers;
        }
    }

    public interface IJobScheduler
    {
        void StartScheduler();
    }
}