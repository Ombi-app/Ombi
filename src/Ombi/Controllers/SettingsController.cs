using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models;
using Ombi.Core.Settings.Models.External;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;

namespace Ombi.Controllers
{
    [Admin]
    public class SettingsController : BaseV1ApiController
    {
        public SettingsController(ISettingsResolver resolver)
        {
            SettingsResolver = resolver;
        }

        private ISettingsResolver SettingsResolver { get; }

        [HttpGet("ombi")]
        public async Task<OmbiSettings> OmbiSettings()
        {
            return await Get<OmbiSettings>();
        }

        [HttpPost("ombi")]
        public async Task<bool> OmbiSettings([FromBody]OmbiSettings ombi)
        {
            return await Save(ombi);
        }

        [HttpGet("plex")]
        public async Task<PlexSettings> PlexSettings()
        {
            return await Get<PlexSettings>();
        }

        [HttpPost("plex")]
        public async Task<bool> PlexSettings([FromBody]PlexSettings plex)
        {
            return await Save(plex);
        }

        [HttpGet("emby")]
        public async Task<EmbySettings> EmbySettings()
        {
            return await Get<EmbySettings>();
        }

        [HttpPost("emby")]
        public async Task<bool> EmbySettings([FromBody]EmbySettings emby)
        {
            return await Save(emby);
        }

        [HttpGet("landingpage")]
        [AllowAnonymous]
        public async Task<LandingPageSettings> LandingPageSettings()
        {
            return await Get<LandingPageSettings>();
        }

        [HttpPost("landingpage")]
        public async Task<bool> LandingPageSettings([FromBody]LandingPageSettings settings)
        {
            return await Save(settings);
        }

        [HttpGet("customization")]
        [AllowAnonymous]
        public async Task<CustomizationSettings> CustomizationSettings()
        {
            return await Get<CustomizationSettings>();
        }

        [HttpPost("customization")]
        public async Task<bool> CustomizationSettings([FromBody]CustomizationSettings settings)
        {
            return await Save(settings);
        }

        [HttpGet("sonarr")]
        [AllowAnonymous]
        public async Task<SonarrSettings> SonarrSettings()
        {
            return await Get<SonarrSettings>();
        }

        [HttpPost("sonarr")]
        public async Task<bool> SonarrSettings([FromBody]SonarrSettings settings)
        {
            return await Save(settings);
        }

        [HttpGet("radarr")]
        [AllowAnonymous]
        public async Task<RadarrSettings> RadarrSettings()
        {
            return await Get<RadarrSettings>();
        }

        [HttpPost("radarr")]
        public async Task<bool> RadarrSettings([FromBody]RadarrSettings settings)
        {
            return await Save(settings);
        }

        private async Task<T> Get<T>()
        {
            var settings = SettingsResolver.Resolve<T>();
            return await settings.GetSettingsAsync();
        }

        private async Task<bool> Save<T>(T settingsModel)
        {
            var settings = SettingsResolver.Resolve<T>();
            return await settings.SaveSettingsAsync(settingsModel);
        }
    }
}
