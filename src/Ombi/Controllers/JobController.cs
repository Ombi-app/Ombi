using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Ombi.Api.Service;
using Ombi.Attributes;
using Ombi.Helpers;
using Ombi.Schedule.Jobs;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Schedule.Jobs.Plex;

namespace Ombi.Controllers
{
    [ApiV1]
    [Admin]
    [Produces("application/json")]
    [ApiController]
    public class JobController : ControllerBase
    {
        public JobController(IOmbiAutomaticUpdater updater, IPlexUserImporter userImporter,
            ICacheService mem, IEmbyUserImporter embyImporter, IPlexContentSync plexContentSync,
            IEmbyContentSync embyContentSync, INewsletterJob newsletter)
        {
            _updater = updater;
            _plexUserImporter = userImporter;
            _embyUserImporter = embyImporter;
            _memCache = mem;
            _plexContentSync = plexContentSync;
            _embyContentSync = embyContentSync;
            _newsletterJob = newsletter;
        }

        private readonly IOmbiAutomaticUpdater _updater;
        private readonly IPlexUserImporter _plexUserImporter;
        private readonly IEmbyUserImporter _embyUserImporter;
        private readonly ICacheService _memCache;
        private readonly IPlexContentSync _plexContentSync;
        private readonly IEmbyContentSync _embyContentSync;
        private readonly INewsletterJob _newsletterJob;

        /// <summary>
        /// Runs the update job
        /// </summary>
        /// <returns></returns>
        [HttpPost("update")]
        public bool ForceUpdate()
        {
            BackgroundJob.Enqueue(() => _updater.Update(null));
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
                var branch = productArray[1];
                var updateAvailable = await _updater.UpdateAvailable(branch, version);

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
            var val = await _memCache.GetOrAdd(CacheKeys.Update, async () =>
            {
                var productArray = _updater.GetVersion();
                var version = productArray[0];
                var branch = productArray[1];
                var updateAvailable = await _updater.UpdateAvailable(branch, version);

                return updateAvailable;
            });
            return val;
        }

        /// <summary>
        /// Runs the Plex User importer
        /// </summary>
        /// <returns></returns>
        [HttpPost("plexuserimporter")]
        public bool PlexUserImporter()
        {
            BackgroundJob.Enqueue(() => _plexUserImporter.Start());
            return true;
        }

        /// <summary>
        /// Runs the Emby User importer
        /// </summary>
        /// <returns></returns>
        [HttpPost("embyuserimporter")]
        public bool EmbyUserImporter()
        {
            BackgroundJob.Enqueue(() => _embyUserImporter.Start());
            return true;
        }

        /// <summary>
        /// Runs the Plex Content Cacher
        /// </summary>
        /// <returns></returns>
        [HttpPost("plexcontentcacher")]
        public bool StartPlexContentCacher()
        {
            BackgroundJob.Enqueue(() => _plexContentSync.CacheContent(false));
            return true;
        }

        /// <summary>
        /// Runs a smaller version of the content cacher
        /// </summary>
        /// <returns></returns>
        [HttpPost("plexrecentlyadded")]
        public bool StartRecentlyAdded()
        {
            BackgroundJob.Enqueue(() => _plexContentSync.CacheContent(true));
            return true;
        }

        /// <summary>
        /// Runs the Emby Content Cacher
        /// </summary>
        /// <returns></returns>
        [HttpPost("embycontentcacher")]
        public bool StartEmbyContentCacher()
        {
            BackgroundJob.Enqueue(() => _embyContentSync.Start());
            return true;
        }

        /// <summary>
        /// Runs the newsletter
        /// </summary>
        /// <returns></returns>
        [HttpPost("newsletter")]
        public bool StartNewsletter()
        {
            BackgroundJob.Enqueue(() => _newsletterJob.Start());
            return true;
        }
    }
}