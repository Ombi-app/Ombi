using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;
using Ombi.Core.Models.UI;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Controllers
{
    /// <summary>
    /// The Settings Controller
    /// </summary>
    /// <seealso cref="Ombi.Controllers.BaseV1ApiController" />
    [Admin]
    public class SettingsController : BaseV1ApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsController" /> class.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        /// <param name="mapper">The mapper.</param>
        public SettingsController(ISettingsResolver resolver, IMapper mapper, INotificationTemplatesRepository templateRepo)
        {
            SettingsResolver = resolver;
            Mapper = mapper;
            TemplateRepository = templateRepo;
        }

        private ISettingsResolver SettingsResolver { get; }
        private IMapper Mapper { get; }
        private INotificationTemplatesRepository TemplateRepository { get; }

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

        /// <summary>
        /// Saves the email notification settings.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost("notifications/email")]
        public async Task<bool> EmailNotificationSettings([FromBody] EmailNotificationsViewModel model)
        {
            // Save the email settings
            var settings = Mapper.Map<EmailNotificationSettings>(model);
            var result = await Save(settings);

            // Save the templates
            await TemplateRepository.UpdateRange(model.NotificationTemplates);

            return result;
        }

        /// <summary>
        /// Gets the Email Notification Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("notifications/email")]
        public async Task<EmailNotificationsViewModel> EmailNotificationSettings()
        {
            var emailSettings = await Get<EmailNotificationSettings>();
            var model = Mapper.Map<EmailNotificationsViewModel>(emailSettings);

            // Lookup to see if we have any templates saved
            model.NotificationTemplates = await BuildTemplates(NotificationAgent.Email);

            return model;
        }

        /// <summary>
        /// Saves the discord notification settings.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost("notifications/email")]
        public async Task<bool> DiscordNotificationSettings([FromBody] DiscordNotificationsViewModel model)
        {
            // Save the email settings
            var settings = Mapper.Map<DiscordNotificationSettings>(model);
            var result = await Save(settings);

            // Save the templates
            await TemplateRepository.UpdateRange(model.NotificationTemplates);

            return result;
        }

        /// <summary>
        /// Gets the discord Notification Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("notifications/email")]
        public async Task<DiscordNotificationsViewModel> DiscordNotificationSettings()
        {
            var emailSettings = await Get<DiscordNotificationSettings>();
            var model = Mapper.Map<DiscordNotificationsViewModel>(emailSettings);

            // Lookup to see if we have any templates saved
            model.NotificationTemplates = await BuildTemplates(NotificationAgent.Discord);

            return model;
        }

        private async Task<List<NotificationTemplates>> BuildTemplates(NotificationAgent agent)
        {
            var templates = await TemplateRepository.GetAllTemplates(agent);
            return templates.ToList();
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
