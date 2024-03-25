using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Radarr;
using Ombi.Api.Radarr.Models;
using Ombi.Api.Radarr.Models.V3;
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

        public RadarrController(
            ISettingsService<RadarrSettings> settings,
            ISettingsService<Radarr4KSettings> radarr4kSettings,
            IRadarrV3Api radarrV3Api)
        {
            _radarrSettings = settings;
            _radarr4KSettings = radarr4kSettings;
            _radarrV3Api = radarrV3Api;
        }

        private readonly ISettingsService<RadarrSettings> _radarrSettings;
        private readonly ISettingsService<Radarr4KSettings> _radarr4KSettings;
        private readonly IRadarrV3Api _radarrV3Api;
        /// <summary>
        /// Gets the Radarr profiles.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("Profiles")]
        [PowerUser]
        public async Task<IActionResult> GetProfiles([FromBody] RadarrSettings settings)
        {
            return Ok(await _radarrV3Api.GetProfiles(settings.ApiKey, settings.FullUri));
        }

        [HttpGet("enabled")]
        [PowerUser]
        public async Task<bool> Enabled()
        {
            var settings = await _radarrSettings.GetSettingsAsync();
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
            return await _radarrV3Api.GetRootFolders(settings.ApiKey, settings.FullUri);
        }

        /// <summary>
        /// Gets the Radarr profiles using the saved settings
        /// <remarks>The data is cached for an hour</remarks>
        /// </summary>
        /// <returns></returns>
        [HttpGet("Profiles")]
        [PowerUser]
        public async Task<IActionResult> GetProfiles()
        {
            var settings = await _radarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return Ok(await _radarrV3Api.GetProfiles(settings.ApiKey, settings.FullUri));
            }
            return Ok(new List<RadarrV3QualityProfile>());
        }

        /// <summary>
        /// Gets the Radarr 4K profiles using the saved settings
        /// <remarks>The data is cached for an hour</remarks>
        /// </summary>
        /// <returns></returns>
        [HttpGet("Profiles/4k")]
        [PowerUser]
        public async Task<IActionResult> GetProfiles4K()
        {
            var settings = await _radarr4KSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return Ok(await _radarrV3Api.GetProfiles(settings.ApiKey, settings.FullUri));
            }
            return Ok(new List<RadarrV3QualityProfile>());
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
            var settings = await _radarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await _radarrV3Api.GetRootFolders(settings.ApiKey, settings.FullUri);
            }
            return null;
        }

        /// <summary>
        /// Gets the Radarr 4K root folders using the saved settings.
        /// <remarks>The data is cached for an hour</remarks>
        /// </summary>
        /// <returns></returns>
        [HttpGet("RootFolders/4k")]
        [PowerUser]
        public async Task<IEnumerable<RadarrRootFolder>> GetRootFolders4K()
        {
            var settings = await _radarr4KSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await _radarrV3Api.GetRootFolders(settings.ApiKey, settings.FullUri);
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
            return await _radarrV3Api.GetTags(settings.ApiKey, settings.FullUri);
        }


        /// <summary>
        /// Gets the Radarr tags
        /// </summary>
        /// <returns></returns>
        [HttpGet("tags")]
        [PowerUser]
        public async Task<IEnumerable<Tag>> GetTags()
        {
            var settings = await _radarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await _radarrV3Api.GetTags(settings.ApiKey, settings.FullUri);
            }

            return null;
        }
    }
}