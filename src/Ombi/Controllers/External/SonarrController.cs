using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Sonarr;
using Ombi.Api.Sonarr.Models;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Settings.Settings.Models.External;

namespace Ombi.Controllers.External
{
    [Admin]
    [ApiV1]
    [Produces("application/json")]
    public class SonarrController : Controller
    {
        public SonarrController(ISonarrApi sonarr, ISettingsService<SonarrSettings> settings)
        {
            SonarrApi = sonarr;
            SonarrSettings = settings;
        }

        private ISonarrApi SonarrApi { get; }
        private ISettingsService<SonarrSettings> SonarrSettings { get; }

        /// <summary>
        /// Gets the Sonarr profiles.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("Profiles")]
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
        public async Task<IEnumerable<SonarrRootFolder>> GetRootFolders([FromBody] SonarrSettings settings)
        {
            return await SonarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
        }

        /// <summary>
        /// Gets the Sonarr profiles.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Profiles")]
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
        public async Task<IEnumerable<SonarrRootFolder>> GetRootFolders()
        {
            var settings = await SonarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await SonarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
            }

            return null;
        }
    }
}