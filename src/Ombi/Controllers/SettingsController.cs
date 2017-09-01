using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.Emby;
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
    [ApiV1]
    public class SettingsController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsController" /> class.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="templateRepo">The templateRepo.</param>
        public SettingsController(ISettingsResolver resolver, IMapper mapper, INotificationTemplatesRepository templateRepo,
            IEmbyApi embyApi)
        {
            SettingsResolver = resolver;
            Mapper = mapper;
            TemplateRepository = templateRepo;
            _embyApi = embyApi;
        }

        private ISettingsResolver SettingsResolver { get; }
        private IMapper Mapper { get; }
        private INotificationTemplatesRepository TemplateRepository { get; }
        private readonly IEmbyApi _embyApi;

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

        [HttpPost("ombi/resetApi")]
        public async Task<string> ResetApiKey()
        {
            var currentSettings = await Get<OmbiSettings>();
            currentSettings.ApiKey = Guid.NewGuid().ToString("N");
            await Save(currentSettings);

            return currentSettings.ApiKey;
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
            foreach (var server in emby.Servers)
            {
                var users = await _embyApi.GetUsers(server.FullUri, server.ApiKey);
                var admin = users.FirstOrDefault(x => x.Policy.IsAdministrator);
                server.AdministratorId = admin?.Id;
            }
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
        /// Save the Authentication settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("authentication")]
        public async Task<bool> AuthenticationsSettings([FromBody]AuthenticationSettings settings)
        {
            return await Save(settings);
        }

        /// <summary>
        /// Gets the Authentication Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("authentication")]
        public async Task<AuthenticationSettings> AuthenticationsSettings()
        {
            return await Get<AuthenticationSettings>();
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
        [HttpPost("notifications/discord")]
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
        [HttpGet("notifications/discord")]
        public async Task<DiscordNotificationsViewModel> DiscordNotificationSettings()
        {
            var emailSettings = await Get<DiscordNotificationSettings>();
            var model = Mapper.Map<DiscordNotificationsViewModel>(emailSettings);

            // Lookup to see if we have any templates saved
            model.NotificationTemplates = await BuildTemplates(NotificationAgent.Discord);

            return model;
        }

        /// <summary>
        /// Saves the pushbullet notification settings.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost("notifications/pushbullet")]
        public async Task<bool> PushbulletNotificationSettings([FromBody] PushbulletNotificationViewModel model)
        {
            // Save the email settings
            var settings = Mapper.Map<PushbulletSettings>(model);
            var result = await Save(settings);

            // Save the templates
            await TemplateRepository.UpdateRange(model.NotificationTemplates);

            return result;
        }

        /// <summary>
        /// Gets the pushbullet Notification Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("notifications/pushbullet")]
        public async Task<PushbulletNotificationViewModel> PushbulletNotificationSettings()
        {
            var settings = await Get<PushbulletSettings>();
            var model = Mapper.Map<PushbulletNotificationViewModel>(settings);

            // Lookup to see if we have any templates saved
            model.NotificationTemplates = await BuildTemplates(NotificationAgent.Pushbullet);

            return model;
        }

        /// <summary>
        /// Saves the pushover notification settings.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost("notifications/pushover")]
        public async Task<bool> PushoverNotificationSettings([FromBody] PushoverNotificationViewModel model)
        {
            // Save the email settings
            var settings = Mapper.Map<PushoverSettings>(model);
            var result = await Save(settings);

            // Save the templates
            await TemplateRepository.UpdateRange(model.NotificationTemplates);

            return result;
        }

        /// <summary>
        /// Gets the pushover Notification Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("notifications/pushover")]
        public async Task<PushoverNotificationViewModel> PushoverNotificationSettings()
        {
            var settings = await Get<PushoverSettings>();
            var model = Mapper.Map<PushoverNotificationViewModel>(settings);

            // Lookup to see if we have any templates saved
            model.NotificationTemplates = await BuildTemplates(NotificationAgent.Pushover);

            return model;
        }


        /// <summary>
        /// Saves the slack notification settings.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost("notifications/slack")]
        public async Task<bool> SlacktNotificationSettings([FromBody] SlackNotificationsViewModel model)
        {
            // Save the email settings
            var settings = Mapper.Map<SlackNotificationSettings>(model);
            var result = await Save(settings);

            // Save the templates
            await TemplateRepository.UpdateRange(model.NotificationTemplates);

            return result;
        }

        /// <summary>
        /// Gets the slack Notification Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("notifications/slack")]
        public async Task<SlackNotificationsViewModel> SlackNotificationSettings()
        {
            var settings = await Get<SlackNotificationSettings>();
            var model = Mapper.Map<SlackNotificationsViewModel>(settings);

            // Lookup to see if we have any templates saved
            model.NotificationTemplates = await BuildTemplates(NotificationAgent.Slack);

            return model;
        }

        /// <summary>
        /// Saves the Mattermost notification settings.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost("notifications/mattermost")]
        public async Task<bool> MattermostNotificationSettings([FromBody] MattermostNotificationsViewModel model)
        {
            // Save the email settings
            var settings = Mapper.Map<MattermostNotificationSettings>(model);
            var result = await Save(settings);

            // Save the templates
            await TemplateRepository.UpdateRange(model.NotificationTemplates);

            return result;
        }

        /// <summary>
        /// Gets the Mattermost Notification Settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("notifications/mattermost")]
        public async Task<MattermostNotificationsViewModel> MattermostNotificationSettings()
        {
            var settings = await Get<MattermostNotificationSettings>();
            var model = Mapper.Map<MattermostNotificationsViewModel>(settings);

            // Lookup to see if we have any templates saved
            model.NotificationTemplates = await BuildTemplates(NotificationAgent.Mattermost);

            return model;
        }

        private async Task<List<NotificationTemplates>> BuildTemplates(NotificationAgent agent)
        {
            var templates = await TemplateRepository.GetAllTemplates(agent);
            return templates.OrderBy(x => x.NotificationType.ToString()).ToList();
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
