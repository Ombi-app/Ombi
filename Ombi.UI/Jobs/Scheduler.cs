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
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Services.Interfaces;
using Ombi.Services.Jobs;
using Ombi.UI.Helpers;
using Quartz;
using Quartz.Impl;

namespace Ombi.UI.Jobs
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
                JobBuilder.Create<PlexContentCacher>().WithIdentity("PlexContentCacher", "PlexCacher").Build(),
                JobBuilder.Create<PlexEpisodeCacher>().WithIdentity("PlexEpisodeCacher", "Plex").Build(),
                JobBuilder.Create<PlexUserChecker>().WithIdentity("PlexUserChecker", "Plex").Build(),
                JobBuilder.Create<SickRageCacher>().WithIdentity("SickRageCacher", "Cache").Build(),
                JobBuilder.Create<SonarrCacher>().WithIdentity("SonarrCacher", "Cache").Build(),
                JobBuilder.Create<CouchPotatoCacher>().WithIdentity("CouchPotatoCacher", "Cache").Build(),
                JobBuilder.Create<WatcherCacher>().WithIdentity("WatcherCacher", "Cache").Build(),
                JobBuilder.Create<StoreBackup>().WithIdentity("StoreBackup", "Database").Build(),
                JobBuilder.Create<StoreCleanup>().WithIdentity("StoreCleanup", "Database").Build(),
                JobBuilder.Create<UserRequestLimitResetter>().WithIdentity("UserRequestLimiter", "Request").Build(),
                JobBuilder.Create<RecentlyAdded>().WithIdentity("RecentlyAddedModel", "Email").Build(),
                JobBuilder.Create<FaultQueueHandler>().WithIdentity("FaultQueueHandler", "Fault").Build(),
                JobBuilder.Create<RadarrCacher>().WithIdentity("RadarrCacher", "Cache").Build(),
            };
            
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

            var jobDetails = jobs as IJobDetail[] ?? jobs.OrderByDescending(x => x.Key.Name).ToArray();
            var triggerDetails = triggers as ITrigger[] ?? triggers.OrderByDescending(x => x.Key.Name).ToArray();

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

            if (s.CouchPotatoCacher == 0)
            {
                s.CouchPotatoCacher = 60;
            }
            if (s.WatcherCacher == 0)
            {
                s.WatcherCacher = 60;
            }
            if (s.PlexAvailabilityChecker == 0)
            {
                s.PlexAvailabilityChecker = 60;
            }
            if (s.PlexEpisodeCacher == 0)
            {
                s.PlexEpisodeCacher = 11;
            }
            if (string.IsNullOrEmpty(s.RecentlyAddedCron))
            {
                var cron =
                    (Quartz.Impl.Triggers.CronTriggerImpl)
                    CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Friday, 7, 0).Build();
                s.RecentlyAddedCron = cron.CronExpressionString; // Weekly CRON at 7 am on Mondays
            }
            if (s.SickRageCacher == 0)
            {
                s.SickRageCacher = 60;
            }
            if (s.SonarrCacher == 0)
            {
                s.SonarrCacher = 60;
            }
            if (s.StoreBackup == 0)
            {
                s.StoreBackup = 24;
            }
            if (s.StoreCleanup == 0)
            {
                s.StoreCleanup = 24;
            }
            if (s.UserRequestLimitResetter == 0)
            {
                s.UserRequestLimitResetter = 12;
            }
            if (s.FaultQueueHandler == 0)
            {
                s.FaultQueueHandler = 6;
            }
            if (s.PlexContentCacher == 0)
            {
                s.PlexContentCacher = 60;
            }
            if (s.PlexUserChecker == 0)
            {
                s.PlexUserChecker = 24;
            }
            if (s.RadarrCacher == 0)
            {
                s.RadarrCacher = 60;
            }

            var triggers = new List<ITrigger>();

            var plexAvailabilityChecker =
                TriggerBuilder.Create()
                    .WithIdentity("PlexAvailabilityChecker", "Plex")
                    .StartAt(DateBuilder.FutureDate(5, IntervalUnit.Minute))
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(s.PlexAvailabilityChecker).RepeatForever())
                    .Build();
            var plexCacher =
                TriggerBuilder.Create()
                    .WithIdentity("PlexContentCacher", "PlexCacher")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(s.PlexContentCacher).RepeatForever())
                    .Build();

            var plexUserChecker =
                TriggerBuilder.Create()
                    .WithIdentity("PlexUserChecker", "Plex")
                    .StartAt(DateBuilder.FutureDate(1, IntervalUnit.Minute))
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(s.PlexUserChecker).RepeatForever())
                    .Build();

            var srCacher =
                TriggerBuilder.Create()
                    .WithIdentity("SickRageCacher", "Cache")
                    .StartAt(DateBuilder.FutureDate(2, IntervalUnit.Minute))
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(s.SickRageCacher).RepeatForever())
                    .Build();

            var sonarrCacher =
                TriggerBuilder.Create()
                    .WithIdentity("SonarrCacher", "Cache")
                    .StartAt(DateBuilder.FutureDate(3, IntervalUnit.Minute))
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(s.SonarrCacher).RepeatForever())
                    .Build();

            var cpCacher =
                TriggerBuilder.Create()
                    .WithIdentity("CouchPotatoCacher", "Cache")
                    .StartAt(DateBuilder.FutureDate(2, IntervalUnit.Minute))
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(s.CouchPotatoCacher).RepeatForever())
                    .Build();

            var watcherCacher =
                TriggerBuilder.Create()
                    .WithIdentity("WatcherCacher", "Cache")
                    //.StartNow()
                    .StartAt(DateBuilder.FutureDate(2, IntervalUnit.Minute))
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(s.WatcherCacher).RepeatForever())
                    .Build();

            var radarrCacher =
    TriggerBuilder.Create()
        .WithIdentity("RadarrCacher", "Cache")
        .StartNow()
        //.StartAt(DateBuilder.FutureDate(2, IntervalUnit.Minute))
        .WithSimpleSchedule(x => x.WithIntervalInMinutes(s.RadarrCacher).RepeatForever())
        .Build();

            var storeBackup =
                TriggerBuilder.Create()
                    .WithIdentity("StoreBackup", "Database")
                    .StartAt(DateBuilder.FutureDate(20, IntervalUnit.Minute))
                    .WithSimpleSchedule(x => x.WithIntervalInHours(s.StoreBackup).RepeatForever())
                    .Build();

            var storeCleanup =
                TriggerBuilder.Create()
                    .WithIdentity("StoreCleanup", "Database")
                    .StartAt(DateBuilder.FutureDate(35, IntervalUnit.Minute))
                    .WithSimpleSchedule(x => x.WithIntervalInHours(s.StoreCleanup).RepeatForever())
                    .Build();

            var userRequestLimiter =
                TriggerBuilder.Create()
                    .WithIdentity("UserRequestLimiter", "Request")
                    .StartAt(DateBuilder.FutureDate(25, IntervalUnit.Minute))
                    // Everything has started on application start, lets wait 5 minutes
                    .WithSimpleSchedule(x => x.WithIntervalInHours(s.UserRequestLimitResetter).RepeatForever())
                    .Build();

            var plexEpCacher =
                TriggerBuilder.Create()
                    .WithIdentity("PlexEpisodeCacher", "Cache")
                    .StartAt(DateBuilder.FutureDate(10, IntervalUnit.Minute))
                    .WithSimpleSchedule(x => x.WithIntervalInHours(s.PlexEpisodeCacher).RepeatForever())
                    .Build();


            var rencentlyAdded =
                TriggerBuilder.Create()
                    .WithIdentity("RecentlyAddedModel", "Email")
                    .StartNow()
                    .WithCronSchedule(s.RecentlyAddedCron)
                    .Build();

            var fault =
                TriggerBuilder.Create()
                    .WithIdentity("FaultQueueHandler", "Fault")
                    //.StartAt(DateBuilder.FutureDate(10, IntervalUnit.Minute))
                    .StartAt(DateBuilder.FutureDate(13, IntervalUnit.Minute))
                    .WithSimpleSchedule(x => x.WithIntervalInHours(s.FaultQueueHandler).RepeatForever())
                    .Build();

            triggers.Add(rencentlyAdded);
            triggers.Add(plexAvailabilityChecker);
            triggers.Add(srCacher);
            triggers.Add(sonarrCacher);
            triggers.Add(cpCacher);
            triggers.Add(watcherCacher);
            triggers.Add(storeBackup);
            triggers.Add(storeCleanup);
            triggers.Add(userRequestLimiter);
            triggers.Add(plexEpCacher);
            triggers.Add(fault);
            triggers.Add(plexCacher);
            triggers.Add(plexUserChecker);
            triggers.Add(radarrCacher);

            return triggers;
        }
    }

    public interface IJobScheduler
    {
        void StartScheduler();
    }
}