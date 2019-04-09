using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Ombi.Core.Settings;
using Ombi.Schedule.Jobs;
using Ombi.Schedule.Jobs.Couchpotato;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Lidarr;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Schedule.Jobs.Plex.Interfaces;
using Ombi.Schedule.Jobs.Radarr;
using Ombi.Schedule.Jobs.SickRage;
using Ombi.Schedule.Jobs.Sonarr;
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

            // Run Quartz
            OmbiQuartz.Start();

            // Run configuration
            AddPlex(s);
            AddEmby(s);
            AddDvrApps(s);
            AddSystem(s);

        }

        private static void AddSystem(JobSettings s)
        {
            OmbiQuartz.Instance.AddJob<IRefreshMetadata>(nameof(IRefreshMetadata), "System", JobSettingsHelper.RefreshMetadata(s));
            OmbiQuartz.Instance.AddJob<IIssuesPurge>(nameof(IIssuesPurge), "System", JobSettingsHelper.IssuePurge(s));
            //OmbiQuartz.Instance.AddJob<IOmbiAutomaticUpdater>(nameof(IOmbiAutomaticUpdater), "System", JobSettingsHelper.Updater(s));
            OmbiQuartz.Instance.AddJob<INewsletterJob>(nameof(INewsletterJob), "System", JobSettingsHelper.Newsletter(s));
            OmbiQuartz.Instance.AddJob<IResendFailedRequests>(nameof(IResendFailedRequests), "System", JobSettingsHelper.ResendFailedRequests(s));
            OmbiQuartz.Instance.AddJob<IMediaDatabaseRefresh>(nameof(IMediaDatabaseRefresh), "System", JobSettingsHelper.MediaDatabaseRefresh(s));
        }

        private static void AddDvrApps(JobSettings s)
        {
            OmbiQuartz.Instance.AddJob<ISonarrSync>(nameof(ISonarrSync), "DVR", JobSettingsHelper.Sonarr(s));
            OmbiQuartz.Instance.AddJob<IRadarrSync>(nameof(IRadarrSync), "DVR", JobSettingsHelper.Radarr(s));
            OmbiQuartz.Instance.AddJob<ICouchPotatoSync>(nameof(ICouchPotatoSync), "DVR", JobSettingsHelper.CouchPotato(s));
            OmbiQuartz.Instance.AddJob<ISickRageSync>(nameof(ISickRageSync), "DVR", JobSettingsHelper.SickRageSync(s));
            OmbiQuartz.Instance.AddJob<ILidarrArtistSync>(nameof(ILidarrArtistSync), "DVR", JobSettingsHelper.LidarrArtistSync(s));
        }

        private static void AddPlex(JobSettings s)
        {
            OmbiQuartz.Instance.AddJob<IPlexContentSync>(nameof(IPlexContentSync), "Plex", JobSettingsHelper.PlexContent(s), new Dictionary<string, string> { { "recentlyAddedSearch", "false" } });
            OmbiQuartz.Instance.AddJob<IPlexContentSync>(nameof(IPlexContentSync) + "RecentlyAdded", "Plex", JobSettingsHelper.PlexContent(s), new Dictionary<string, string> { { "recentlyAddedSearch", "true" } });
            OmbiQuartz.Instance.AddJob<IPlexRecentlyAddedSync>(nameof(IPlexRecentlyAddedSync), "Plex", JobSettingsHelper.PlexRecentlyAdded(s));
            OmbiQuartz.Instance.AddJob<IPlexUserImporter>(nameof(IPlexUserImporter), "Plex", JobSettingsHelper.UserImporter(s));
        }

        private static void AddEmby(JobSettings s)
        {
            OmbiQuartz.Instance.AddJob<IEmbyContentSync>(nameof(IEmbyContentSync), "Emby", JobSettingsHelper.EmbyContent(s));
            OmbiQuartz.Instance.AddJob<IEmbyUserImporter>(nameof(IEmbyUserImporter), "Emby", JobSettingsHelper.UserImporter(s));
        }
    }
}