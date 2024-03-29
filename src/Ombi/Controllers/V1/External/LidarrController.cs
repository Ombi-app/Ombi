﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Lidarr;
using Ombi.Api.Lidarr.Models;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;

namespace Ombi.Controllers.V1.External
{
   [ApiV1]
   [Produces("application/json")]
    public class LidarrController : Controller
    {
        public LidarrController(ILidarrApi lidarr, ISettingsService<LidarrSettings> settings,
            ICacheService mem)
        {
            _lidarrApi = lidarr;
            _lidarrSettings = settings;
            Cache = mem;
        }

        private readonly ILidarrApi _lidarrApi;
        private readonly ISettingsService<LidarrSettings> _lidarrSettings;
        private ICacheService Cache { get; }

        [HttpGet("enabled")]
        public async Task<bool> Enabled()
        {
            var settings = await _lidarrSettings.GetSettingsAsync();
            return settings.Enabled;
        }

        /// <summary>
        /// Gets the Lidarr profiles.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [PowerUser]
        [HttpPost("Profiles")]
        public async Task<IEnumerable<LidarrProfile>> GetProfiles([FromBody] LidarrSettings settings)
        {
            return await _lidarrApi.GetProfiles(settings.ApiKey, settings.FullUri);
        }

        /// <summary>
        /// Gets the Lidarr root folders.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [PowerUser]
        [HttpPost("RootFolders")]
        public async Task<IEnumerable<LidarrRootFolder>> GetRootFolders([FromBody] LidarrSettings settings)
        {
            return await _lidarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
        }

        /// <summary>
        /// Gets the Lidarr metadata profiles.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [PowerUser]
        [HttpPost("Metadata")]
        public async Task<IEnumerable<MetadataProfile>> GetMetadataProfiles([FromBody] LidarrSettings settings)
        {
            return await _lidarrApi.GetMetadataProfile(settings.ApiKey, settings.FullUri);
        }

        /// <summary>
        /// Gets the Lidarr profiles using the saved settings
        /// <remarks>The data is cached for an hour</remarks>
        /// </summary>
        /// <returns></returns>
        [PowerUser]
        [HttpGet("Profiles")]
        public async Task<IEnumerable<LidarrProfile>> GetProfiles()
        {
            return await Cache.GetOrAddAsync(CacheKeys.LidarrQualityProfiles, async () =>
            {
                var settings = await _lidarrSettings.GetSettingsAsync();
                if (settings.Enabled)
                {
                    return await _lidarrApi.GetProfiles(settings.ApiKey, settings.FullUri);
                }
                return null;
            }, DateTimeOffset.Now.AddHours(1));
        }

        /// <summary>
        /// Gets the Lidarr root folders using the saved settings.
        /// <remarks>The data is cached for an hour</remarks>
        /// </summary>
        /// <returns></returns>
        [PowerUser]
        [HttpGet("RootFolders")]
        public async Task<IEnumerable<LidarrRootFolder>> GetRootFolders()
        {
            return await Cache.GetOrAddAsync(CacheKeys.LidarrRootFolders, async () =>
            {
                var settings = await _lidarrSettings.GetSettingsAsync();
                if (settings.Enabled)
                {
                    return await _lidarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
                }
                return null;
            }, DateTimeOffset.Now.AddHours(1));
        }
    }
}