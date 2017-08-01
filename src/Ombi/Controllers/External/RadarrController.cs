using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Radarr;
using Ombi.Api.Radarr.Models;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models.External;

namespace Ombi.Controllers.External
{
   [Admin]
   [ApiV1]
    public class RadarrController : Controller
    {
        public RadarrController(IRadarrApi radarr, ISettingsService<RadarrSettings> settings)
        {
            RadarrApi = radarr;
            RadarrSettings = settings;
        }

        private IRadarrApi RadarrApi { get; }
        private ISettingsService<RadarrSettings> RadarrSettings { get; }

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
    }
}