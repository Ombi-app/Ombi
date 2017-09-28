using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Ombi.Api.Service;
using Ombi.Attributes;
using Ombi.Helpers;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Schedule.Ombi;

namespace Ombi.Controllers
{
    [ApiV1]
    [Admin]
    [Produces("application/json")]
    public class JobController : Controller
    {
        public JobController(IOmbiAutomaticUpdater updater, IPlexUserImporter userImporter,
            IMemoryCache mem)
        {
            _updater = updater;
            _plexUserImporter = userImporter;
            _memCache = mem;
        }

        private readonly IOmbiAutomaticUpdater _updater;
        private readonly IPlexUserImporter _plexUserImporter;
        private readonly IMemoryCache _memCache;

        [HttpPost("update")]
        public bool ForceUpdate()
        {
            BackgroundJob.Enqueue(() => _updater.Update(null));
            return true;
        }

        [HttpGet("update")]
        public async Task<bool> CheckForUpdate()
        {
            var productArray = _updater.GetVersion();
            var version = productArray[0];
            var branch = productArray[1];
            var updateAvailable = await _updater.UpdateAvailable(branch, version);

            return updateAvailable;
        }

        [HttpGet("updateCached")]
        public async Task<bool> CheckForUpdateCached()
        {
            var val = await _memCache.GetOrCreateAsync(CacheKeys.Update, async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
                var productArray = _updater.GetVersion();
                var version = productArray[0];
                var branch = productArray[1];
                var updateAvailable = await _updater.UpdateAvailable(branch, version);

                return updateAvailable;
            });
            return val;
        }

        [HttpPost("plexuserimporter")]
        public bool PlexUserImporter()
        {
            BackgroundJob.Enqueue(() => _plexUserImporter.Start());
            return true;
        }
    }
}