﻿#region Copyright
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

using PlexRequests.Services.Jobs;

using Quartz;
using Quartz.Impl;

namespace PlexRequests.UI.Jobs
{
    internal sealed class Scheduler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ISchedulerFactory _factory;

        public Scheduler()
        {
            _factory = new StdSchedulerFactory();
        }

        private IEnumerable<IJobDetail> CreateJobs()
        {
            var jobs = new List<IJobDetail>();

            var plex = JobBuilder.Create<PlexAvailabilityChecker>().WithIdentity("PlexAvailabilityChecker", "Plex").Build();
            var sickrage = JobBuilder.Create<SickRageCacher>().WithIdentity("SickRageCacher", "Cache").Build();
            var sonarr = JobBuilder.Create<SonarrCacher>().WithIdentity("SonarrCacher", "Cache").Build();
            var cp = JobBuilder.Create<CouchPotatoCacher>().WithIdentity("CouchPotatoCacher", "Cache").Build();
            var store = JobBuilder.Create<StoreBackup>().WithIdentity("StoreBackup", "Backup").Build();

            jobs.Add(plex);
            jobs.Add(sickrage);
            jobs.Add(sonarr);
            jobs.Add(cp);
            jobs.Add(store);

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
            var triggers = new List<ITrigger>();

            var plexAvailabilityChecker =
                TriggerBuilder.Create()
                              .WithIdentity("PlexAvailabilityChecker", "Plex")
                              .StartNow()
                              .WithSimpleSchedule(x => x.WithIntervalInMinutes(10).RepeatForever())
                              .Build();

            var srCacher =
                TriggerBuilder.Create()
                              .WithIdentity("SickRageCacher", "Cache")
                              .StartNow()
                              .WithSimpleSchedule(x => x.WithIntervalInMinutes(10).RepeatForever())
                              .Build();

            var sonarrCacher =
                TriggerBuilder.Create()
                              .WithIdentity("SonarrCacher", "Cache")
                              .StartNow()
                              .WithSimpleSchedule(x => x.WithIntervalInMinutes(10).RepeatForever())
                              .Build();

            var cpCacher =
                TriggerBuilder.Create()
                              .WithIdentity("CouchPotatoCacher", "Cache")
                              .StartNow()
                              .WithSimpleSchedule(x => x.WithIntervalInMinutes(10).RepeatForever())
                              .Build();

            var storeBackup =
                TriggerBuilder.Create()
                              .WithIdentity("StoreBackup", "Backup")
                              .StartNow()
                              .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever())
                              .Build();


            triggers.Add(plexAvailabilityChecker);
            triggers.Add(srCacher);
            triggers.Add(sonarrCacher);
            triggers.Add(cpCacher);
            triggers.Add(storeBackup);

            return triggers;
        }
    }
}