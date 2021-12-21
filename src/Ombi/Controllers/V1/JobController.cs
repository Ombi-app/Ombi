﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;
using Ombi.Helpers;
using Ombi.Schedule.Jobs;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Jellyfin;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Schedule.Jobs.Plex.Interfaces;
using Ombi.Schedule.Jobs.Radarr;
using Quartz;

namespace Ombi.Controllers.V1
{
    [ApiV1]
    [Admin]
    [Produces("application/json")]
    [ApiController]
    public class JobController : ControllerBase
    {
        public JobController(IOmbiAutomaticUpdater updater, ICacheService mem)
        {
            _updater = updater;
            _memCache = mem;
        }

        private readonly IOmbiAutomaticUpdater _updater;
        private readonly ICacheService _memCache;
        /// <summary>
        /// Runs the update job
        /// </summary>
        /// <returns></returns>
        [HttpPost("update")]
        public async Task<bool> ForceUpdate()
        {

            await OmbiQuartz.TriggerJob(nameof(IOmbiAutomaticUpdater), "System");
            return true;
        }

        /// <summary>
        /// Checks for an update
        /// </summary>
        /// <returns></returns>
        [HttpGet("update")]
        public async Task<bool> CheckForUpdate()
        {
            try
            {
                var productArray = _updater.GetVersion();
                var version = productArray[0];
                var updateAvailable = await _updater.UpdateAvailable(version);

                return updateAvailable;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("updateCached")]
        public async Task<bool> CheckForUpdateCached()
        {
            var val = await _memCache.GetOrAddAsync(CacheKeys.Update, async () =>
            {
                var productArray = _updater.GetVersion();

                    var version = productArray[0];

                    var updateAvailable = await _updater.UpdateAvailable( version);
                

                return true;
            });
            return val;
        }

        /// <summary>
        /// Runs the Plex User importer
        /// </summary>
        /// <returns></returns>
        [HttpPost("plexuserimporter")]
        public async Task<bool> PlexUserImporter()
        {
            await OmbiQuartz.TriggerJob(nameof(IPlexUserImporter), "Plex");
            return true;
        }

        /// <summary>
        /// Runs the Emby User importer
        /// </summary>
        /// <returns></returns>
        [HttpPost("embyuserimporter")]
        public async Task<bool> EmbyUserImporter()
        {
            await OmbiQuartz.TriggerJob(nameof(IEmbyUserImporter), "Emby");
            return true;
        }

        /// <summary>
        /// Runs the Jellyfin User importer
        /// </summary>
        /// <returns></returns>
        [HttpPost("jellyfinuserimporter")]
        public async Task<bool> JellyfinUserImporter()
        {
            await OmbiQuartz.TriggerJob(nameof(IJellyfinUserImporter), "Jellyfin");
            return true;
        }

        /// <summary>
        /// Runs the Plex Content Cacher
        /// </summary>
        /// <returns></returns>
        [HttpPost("plexcontentcacher")]
        public async Task<bool> StartPlexContentCacher()
        {
            await OmbiQuartz.Scheduler.TriggerJob(new JobKey(nameof(IPlexContentSync), "Plex"), new JobDataMap(new Dictionary<string, string> { { "recentlyAddedSearch", "false" } }));
            return true;
        }

        /// <summary>
        /// Clear out the media server and resync
        /// </summary>
        /// <returns></returns>
        [HttpPost("clearmediaserverdata")]
        public async Task<bool> ClearMediaServerData()
        {
            await OmbiQuartz.Scheduler.TriggerJob(new JobKey(nameof(IMediaDatabaseRefresh), "System"));
            return true;
        }

        /// <summary>
        /// Runs a smaller version of the content cacher
        /// </summary>
        /// <returns></returns>
        [HttpPost("plexrecentlyadded")]
        public async Task<bool> StartRecentlyAdded()
        {
            await OmbiQuartz.Scheduler.TriggerJob(new JobKey(nameof(IPlexContentSync) + "RecentlyAdded", "Plex"), new JobDataMap(new Dictionary<string, string> { { "recentlyAddedSearch", "true" } }));
            return true;
        }

        /// <summary>
        /// Runs the Emby Content Cacher
        /// </summary>
        /// <returns></returns>
        [HttpPost("embycontentcacher")]
        public async Task<bool> StartEmbyContentCacher()
        {
            await OmbiQuartz.Scheduler.TriggerJob(new JobKey(nameof(IEmbyContentSync), "Emby"), new JobDataMap(new Dictionary<string, string> { { JobDataKeys.EmbyRecentlyAddedSearch, "false" } }));
            return true;
        }

        /// <summary>
        /// Runs a smaller version of the content cacher
        /// </summary>
        /// <returns></returns>
        [HttpPost("embyrecentlyadded")]
        public async Task<bool> StartEmbyRecentlyAdded()
        {
            await OmbiQuartz.Scheduler.TriggerJob(new JobKey(nameof(IEmbyContentSync) + "RecentlyAdded", "Emby"), new JobDataMap(new Dictionary<string, string> { { JobDataKeys.EmbyRecentlyAddedSearch, "true" } }));
            return true;
        }

        /// <summary>
        /// Runs the Jellyfin Content Cacher
        /// </summary>
        /// <returns></returns>
        [HttpPost("jellyfincontentcacher")]
        public async Task<bool> StartJellyfinContentCacher()
        {
            await OmbiQuartz.TriggerJob(nameof(IJellyfinContentSync), "Jellyfin");
            return true;
        }

        /// <summary>
        /// Runs the Arr Availability Checker
        /// </summary>
        /// <returns></returns>
        [HttpPost("arrAvailability")]
        public async Task<bool> StartArrAvailabiltityChecker()
        {
            await OmbiQuartz.TriggerJob(nameof(IArrAvailabilityChecker), "DVR");
            return true;
        }


        [HttpPost("autodeleterequests")]
        public async Task<bool> StartAutoDeleteRequests()
        {
            await OmbiQuartz.TriggerJob(nameof(IAutoDeleteRequests), "System");
            return true;
        }

        /// <summary>
        /// Runs the newsletter
        /// </summary>
        /// <returns></returns>
        [HttpPost("newsletter")]
        public async Task<bool> StartNewsletter()
        {
            await OmbiQuartz.TriggerJob(nameof(INewsletterJob), "System");
            return true;
        }
    }
}
