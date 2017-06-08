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

        /// <summary>
        /// Gets the Ombi settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("ombi")]
        public async Task<OmbiSettings> OmbiSettings()
        {
            return await Get<OmbiSettings>();
        }

        /// <summary>
        /// Save the Ombi settings.
        /// </summary>
        /// <param name="ombi">The ombi.</param>
        /// <returns></returns>
        [HttpPost("ombi")]
        public async Task<bool> OmbiSettings([FromBody]OmbiSettings ombi)
        {
            return await Save(ombi);
        }

        /// <summary>
        /// Gets the Plex Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("plex")]
        public async Task<PlexSettings> PlexSettings()
        {
            return await Get<PlexSettings>();
        }

        /// <summary>
        /// Save the Plex settings.
        /// </summary>
        /// <param name="plex">The plex.</param>
        /// <returns></returns>
        [HttpPost("plex")]
        public async Task<bool> PlexSettings([FromBody]PlexSettings plex)
        {
            return await Save(plex);
        }

        /// <summary>
        /// Gets the Emby Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("emby")]
        public async Task<EmbySettings> EmbySettings()
        {
            return await Get<EmbySettings>();
        }

        /// <summary>
        /// Save the Emby settings.
        /// </summary>
        /// <param name="emby">The emby.</param>
        /// <returns></returns>
        [HttpPost("emby")]
        public async Task<bool> EmbySettings([FromBody]EmbySettings emby)
        {
            return await Save(emby);
        }

        /// <summary>
        /// Gets the Landing Page Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("landingpage")]
        [AllowAnonymous]
        public async Task<LandingPageSettings> LandingPageSettings()
        {
            return await Get<LandingPageSettings>();
        }

        /// <summary>
        /// Save the Landing Page settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("landingpage")]
        public async Task<bool> LandingPageSettings([FromBody]LandingPageSettings settings)
        {
            return await Save(settings);
        }

        /// <summary>
        /// Gets the Customization Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("customization")]
        [AllowAnonymous]
        public async Task<CustomizationSettings> CustomizationSettings()
        {
            return await Get<CustomizationSettings>();
        }

        /// <summary>
        /// Save the Customization settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("customization")]
        public async Task<bool> CustomizationSettings([FromBody]CustomizationSettings settings)
        {
            return await Save(settings);
        }

        /// <summary>
        /// Gets the Sonarr Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("sonarr")]
        public async Task<SonarrSettings> SonarrSettings()
        {
            return await Get<SonarrSettings>();
        }

        /// <summary>
        /// Save the Sonarr settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("sonarr")]
        public async Task<bool> SonarrSettings([FromBody]SonarrSettings settings)
        {
            return await Save(settings);
        }

        /// <summary>
        /// Gets the Radarr Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("radarr")]
        public async Task<RadarrSettings> RadarrSettings()
        {
            return await Get<RadarrSettings>();
        }

        /// <summary>
        /// Save the Radarr settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
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
