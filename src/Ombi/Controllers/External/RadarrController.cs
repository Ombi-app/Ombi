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
    public class RadarrController : BaseV1ApiController
    {
        public RadarrController(IRadarrApi radarr, ISettingsService<RadarrSettings> settings)
        {
            RadarrApi = radarr;
            RadarrSettings = settings;
        }

        private IRadarrApi RadarrApi { get; }
        private ISettingsService<RadarrSettings> RadarrSettings { get; }

        [HttpPost("Profiles")]
        public async Task<IEnumerable<RadarrProfile>> GetProfiles([FromBody] RadarrSettings settings)
        {
            return await RadarrApi.GetProfiles(settings.ApiKey, settings.FullUri);
        }

        [HttpPost("RootFolders")]
        public async Task<IEnumerable<RadarrRootFolder>> GetRootFolders([FromBody] RadarrSettings settings)
        {
            return await RadarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
        }
    }
}