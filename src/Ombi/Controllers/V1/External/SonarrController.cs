using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Sonarr;
using Ombi.Api.Sonarr.Models;
using Ombi.Api.Sonarr.Models.V3;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models.External;

namespace Ombi.Controllers.V1.External
{
    [Authorize]
    [ApiV1]
    [Produces("application/json")]
    public class SonarrController : Controller
    {
        public SonarrController(ISonarrApi sonarr, ISonarrV3Api sonarrv3, ISettingsService<SonarrSettings> settings)
        {
            SonarrApi = sonarr;
            SonarrV3Api = sonarrv3;
            SonarrSettings = settings;
        }

        private ISonarrApi SonarrApi { get; }
        private ISonarrV3Api SonarrV3Api { get; }
        private ISettingsService<SonarrSettings> SonarrSettings { get; }

        /// <summary>
        /// Gets the Sonarr profiles.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("Profiles")]
        [PowerUser]
        public async Task<IEnumerable<SonarrProfile>> GetProfiles([FromBody] SonarrSettings settings)
        {
            return await SonarrApi.GetProfiles(settings.ApiKey, settings.FullUri);
        }

        /// <summary>
        /// Gets the Sonarr root folders.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("RootFolders")]
        [PowerUser]
        public async Task<IEnumerable<SonarrRootFolder>> GetRootFolders([FromBody] SonarrSettings settings)
        {
            return await SonarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
        }

        /// <summary>
        /// Gets the Sonarr profiles.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Profiles")]
        [PowerUser]
        public async Task<IEnumerable<SonarrProfile>> GetProfiles()
        {
            var settings = await SonarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await SonarrApi.GetProfiles(settings.ApiKey, settings.FullUri);
            }
            return null;
        }

        /// <summary>
        /// Gets the Sonarr root folders.
        /// </summary>
        /// <returns></returns>
        [HttpGet("RootFolders")]
        [PowerUser]
        public async Task<IEnumerable<SonarrRootFolder>> GetRootFolders()
        {
            var settings = await SonarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await SonarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
            }

            return null;
        }

        /// <summary>
        /// Gets the Sonarr V3 language profiles
        /// </summary>
        /// <returns></returns>
        [HttpGet("v3/LanguageProfiles")]
        [PowerUser]
        public async Task<IEnumerable<LanguageProfiles>> GetLanguageProfiles()
        {
            var settings = await SonarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await SonarrV3Api.LanguageProfiles(settings.ApiKey, settings.FullUri);
            }

            return null;
        }

        /// <summary>
        /// Gets the Sonarr tags
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("tags")]
        [PowerUser]
        public async Task<IEnumerable<Tag>> GetTags([FromBody] SonarrSettings settings)
        {
            return await SonarrApi.GetTags(settings.ApiKey, settings.FullUri);
        }


        /// <summary>
        /// Gets the Sonarr tags
        /// </summary>
        /// <returns></returns>
        [HttpGet("tags")]
        [PowerUser]
        public async Task<IEnumerable<Tag>> GetTags()
        {
            var settings = await SonarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await SonarrApi.GetTags(settings.ApiKey, settings.FullUri);
            }

            return null;
        }

        /// <summary>
        /// Gets the Sonarr V3 language profiles
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("v3/LanguageProfiles")]
        [PowerUser]
        public async Task<IEnumerable<LanguageProfiles>> GetLanguageProfiles([FromBody] SonarrSettings settings)
        {
            return await SonarrV3Api.LanguageProfiles(settings.ApiKey, settings.FullUri);
        }

        [HttpGet("enabled")]
        [PowerUser]
        public async Task<bool> Enabled()
        {
            var settings = await SonarrSettings.GetSettingsAsync();
            return settings.Enabled;
        }

    }
}