using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Radarr;
using Ombi.Api.Radarr.Models;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;

namespace Ombi.Controllers.V1.External
{
    [Authorize]
    [ApiV1]
    [ApiController]
    [Produces("application/json")]
    public class RadarrController : ControllerBase
    { 
        public RadarrController(IRadarrApi radarr, ISettingsService<RadarrSettings> settings,
            ICacheService mem)
        {
            RadarrApi = radarr;
            RadarrSettings = settings;
            Cache = mem;
        }

        private IRadarrApi RadarrApi { get; }
        private ISettingsService<RadarrSettings> RadarrSettings { get; }
        private ICacheService Cache { get; }
        /// <summary>
        /// Gets the Radarr profiles.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("Profiles")]
        [PowerUser]
        public async Task<IEnumerable<RadarrProfile>> GetProfiles([FromBody] RadarrSettings settings)
        {
            return await RadarrApi.GetProfiles(settings.ApiKey, settings.FullUri);
        }

        [HttpGet("enabled")]
        [PowerUser]
        public async Task<bool> Enabled()
        {
            var settings = await RadarrSettings.GetSettingsAsync();
            return settings.Enabled;
        }

        /// <summary>
        /// Gets the Radarr root folders.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("RootFolders")]
        [PowerUser]
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
        [PowerUser]
        public async Task<IEnumerable<RadarrProfile>> GetProfiles()
        {
            var settings = await RadarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await RadarrApi.GetProfiles(settings.ApiKey, settings.FullUri);
            }
            return null;
        }

        /// <summary>
        /// Gets the Radarr root folders using the saved settings.
        /// <remarks>The data is cached for an hour</remarks>
        /// </summary>
        /// <returns></returns>
        [HttpGet("RootFolders")]
        [PowerUser]
        public async Task<IEnumerable<RadarrRootFolder>> GetRootFolders()
        {
            var settings = await RadarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await RadarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
            }
            return null;
        }
        
        /// <summary>
        /// Gets the Radarr tags
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("tags")]
        [PowerUser]
        public async Task<IEnumerable<Tag>> GetTags([FromBody] SonarrSettings settings)
        {
            return await RadarrApi.GetTags(settings.ApiKey, settings.FullUri);
        }

                
        /// <summary>
        /// Gets the Radarr tags
        /// </summary>
        /// <returns></returns>
        [HttpGet("tags")]
        [PowerUser]
        public async Task<IEnumerable<Tag>> GetTags()
        {
            var settings = await RadarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await RadarrApi.GetTags(settings.ApiKey, settings.FullUri);
            }

            return null;
        }
    }
}