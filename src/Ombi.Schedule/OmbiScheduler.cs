using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Ombi.Core.Settings;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Settings.Settings.Models;
using Quartz;
using Quartz.Spi;

namespace Ombi.Schedule
{
    public static class OmbiScheduler
    {
        //public void Setup()
        //{
        //    CreateJobDefinitions();
        //}

        //public void CreateJobDefinitions()
        //{
        //    var contentSync = JobBuilder.Create<PlexContentSync>()
        //        .UsingJobData("recentlyAddedSearch", false)
        //        .WithIdentity(nameof(PlexContentSync), "Plex")
        //        .Build();

        //    var recentlyAdded = JobBuilder.Create<PlexContentSync>()
        //        .UsingJobData("recentlyAddedSearch", true)
        //        .WithIdentity("PlexRecentlyAdded", "Plex")
        //        .Build();
        //}

        public static void UseQuartz(this IApplicationBuilder app)
        {
            // Job Factory through IOC container
            var jobFactory = (IJobFactory)app.ApplicationServices.GetService(typeof(IJobFactory));
            var service = (ISettingsService<JobSettings>)app.ApplicationServices.GetService(typeof(ISettingsService<JobSettings>));
            var s = service.GetSettings();
            // Set job factory
            OmbiQuartz.Instance.UseJobFactory(jobFactory);

            // Run configuration
            OmbiQuartz.Instance.AddJob<PlexContentSync>(nameof(PlexContentSync), "Plex", JobSettingsHelper.PlexContent(s), new Dictionary<string, string>{{ "recentlyAddedSearch", "false" } });
            OmbiQuartz.Instance.AddJob<PlexContentSync>(nameof(PlexContentSync), "Plex", JobSettingsHelper.PlexContent(s), new Dictionary<string, string> { { "recentlyAddedSearch", "true" } });

            // Run Quartz
            OmbiQuartz.Start();
        }
    }
}