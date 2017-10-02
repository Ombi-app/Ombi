using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Ombi.Api.Radarr;
using Ombi.Api.Radarr.Models;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;

namespace Ombi.Controllers.External
{
   [PowerUser]
   [ApiV1]
   [Produces("application/json")]
    public class RadarrController : Controller
    {
        public RadarrController(IRadarrApi radarr, ISettingsService<RadarrSettings> settings,
            IMemoryCache mem)
        {
            RadarrApi = radarr;
            RadarrSettings = settings;
            Cache = mem;
        }

        private IRadarrApi RadarrApi { get; }
        private ISettingsService<RadarrSettings> RadarrSettings { get; }
        private IMemoryCache Cache { get; }
        /// <summary>
        /// Gets the Radarr profiles.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("Profiles")]
        public async Task<IEnumerable<RadarrProfile>> GetProfiles([FromBody] RadarrSettings settings)
        {
            return await RadarrApi.GetProfiles(settings.ApiKey, settings.FullUri);
        }

        /// <summary>
        /// Gets the Radar root folders.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("RootFolders")]
        public async Task<IEnumerable<RadarrRootFolder>> GetRootFolders([FromBody] RadarrSettings settings)
        {
            return await RadarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
        }

        /// <summary>
        /// Gets the Radarr profiles using the saved settings
        /// <remarks>The data is cached for an hour</remarks>
        /// </summary>
        /// <returns></returns>
        [HttpGet("Profiles")]
        public async Task<IEnumerable<RadarrProfile>> GetProfiles()
        {
            return await Cache.GetOrCreate(CacheKeys.RadarrQualityProfiles, async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
                var settings = await RadarrSettings.GetSettingsAsync();
                return await RadarrApi.GetProfiles(settings.ApiKey, settings.FullUri);
            });
        }

        /// <summary>
        /// Gets the Radar root folders using the saved settings.
        /// <remarks>The data is cached for an hour</remarks>
        /// </summary>
        /// <returns></returns>
        [HttpGet("RootFolders")]
        public async Task<IEnumerable<RadarrRootFolder>> GetRootFolders()
        {
            return await Cache.GetOrCreate(CacheKeys.RadarrRootProfiles, async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
                var settings = await RadarrSettings.GetSettingsAsync();
                return await RadarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
            });
        }
    }
}