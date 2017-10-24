using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ombi.Api.CouchPotato;
using Ombi.Api.Emby;
using Ombi.Api.Plex;
using Ombi.Api.Radarr;
using Ombi.Api.Sonarr;
using Ombi.Attributes;
using Ombi.Core.Notifications;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Notifications;
using Ombi.Notifications.Agents;
using Ombi.Notifications.Models;
using Ombi.Notifications.Templates;
using Ombi.Settings.Settings.Models.External;
using Ombi.Settings.Settings.Models.Notifications;

namespace Ombi.Controllers.External
{
    /// <summary>
    /// The Tester Controller
    /// </summary>
    [Admin]
    [ApiV1]
    [Produces("application/json")]
    public class TesterController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TesterController" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="notification">The notification.</param>
        /// <param name="emailN">The notification.</param>
        /// <param name="pushbullet">The pushbullet.</param>
        /// <param name="slack">The slack.</param>
        /// <param name="plex">The plex.</param>
        /// <param name="emby">The emby.</param>
        /// <param name="radarr">The radarr.</param>
        /// <param name="sonarr">The sonarr.</param>
        /// <param name="po">The pushover.</param>
        /// <param name="mm">The mattermost.</param>
        /// <param name="log">The logger.</param>
        /// <param name="provider">The email provider</param>
        /// <param name="cpApi">The couch potato API</param>
        public TesterController(INotificationService service, IDiscordNotification notification, IEmailNotification emailN,
            IPushbulletNotification pushbullet, ISlackNotification slack, IPushoverNotification po, IMattermostNotification mm,
            IPlexApi plex, IEmbyApi emby, IRadarrApi radarr, ISonarrApi sonarr, ILogger<TesterController> log, IEmailProvider provider,
            ICouchPotatoApi cpApi)
        {
            Service = service;
            DiscordNotification = notification;
            EmailNotification = emailN;
            PushbulletNotification = pushbullet;
            SlackNotification = slack;
            PushoverNotification = po;
            MattermostNotification = mm;
            PlexApi = plex;
            RadarrApi = radarr;
            EmbyApi = emby;
            SonarrApi = sonarr;
            Log = log;
            EmailProvider = provider;
            CouchPotatoApi = cpApi;
        }

        private INotificationService Service { get; }
        private IDiscordNotification DiscordNotification { get; }
        private IEmailNotification EmailNotification { get; }
        private IPushbulletNotification PushbulletNotification { get; }
        private ISlackNotification SlackNotification { get; }
        private IPushoverNotification PushoverNotification { get; }
        private IMattermostNotification MattermostNotification { get; }
        private IPlexApi PlexApi { get; }
        private IRadarrApi RadarrApi { get; }
        private IEmbyApi EmbyApi { get; }
        private ISonarrApi SonarrApi { get; }
        private ICouchPotatoApi CouchPotatoApi { get; }
        private ILogger<TesterController> Log { get; }
        private IEmailProvider EmailProvider { get; }


        /// <summary>
        /// Sends a test message to discord using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("discord")]
        public bool Discord([FromBody] DiscordNotificationSettings settings)
        {
            settings.Enabled = true;
            DiscordNotification.NotifyAsync(
                new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

            return true;
        }

        /// <summary>
        /// Sends a test message to Pushbullet using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("pushbullet")]
        public bool Pushbullet([FromBody] PushbulletSettings settings)
        {
            settings.Enabled = true;
            PushbulletNotification.NotifyAsync(
                new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

            return true;
        }

        /// <summary>
        /// Sends a test message to Pushover using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("pushover")]
        public bool Pushover([FromBody] PushoverSettings settings)
        {
            settings.Enabled = true;
            PushoverNotification.NotifyAsync(
                new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

            return true;
        }

        /// <summary>
        /// Sends a test message to mattermost using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("mattermost")]
        public bool Mattermost([FromBody] MattermostNotificationSettings settings)
        {
            settings.Enabled = true;
            MattermostNotification.NotifyAsync(
                new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

            return true;
        }


        /// <summary>
        /// Sends a test message to Slack using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("slack")]
        public bool Slack([FromBody] SlackNotificationSettings settings)
        {
            settings.Enabled = true;
            SlackNotification.NotifyAsync(
                new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

            return true;
        }

        /// <summary>
        /// Sends a test message via email to the admin email using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("email")]
        public async Task<bool> Email([FromBody] EmailNotificationSettings settings)
        {
            try
            {
                var message = new NotificationMessage
                {
                    Message = "This is just a test! Success!",
                    Subject = $"Ombi: Test",
                    To = settings.AdminEmail,
                };

                message.Other.Add("PlainTextBody", "This is just a test! Success!");
                await EmailProvider.SendAdHoc(message, settings);
            }
            catch (Exception e)
            {
                Log.LogWarning(e, "Exception when testing Email Notifications");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if we can connect to Plex with the provided settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [HttpPost("plex")]
        public async Task<bool> Plex([FromBody] PlexServers settings)
        {
            try
            {
                var result = await PlexApi.GetStatus(settings.PlexAuthToken, settings.FullUri);
                return result?.MediaContainer?.version != null;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Plex");
                return false;
            }
        }

        /// <summary>
        /// Checks if we can connect to Emby with the provided settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [HttpPost("emby")]
        public async Task<bool> Emby([FromBody] EmbyServers settings)
        {
            try
            {

                var result = await EmbyApi.GetUsers(settings.FullUri, settings.ApiKey);
                return result.Any();
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Emby");
                return false;
            }
        }

        /// <summary>
        /// Checks if we can connect to Radarr with the provided settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [HttpPost("radarr")]
        public async Task<bool> Radarr([FromBody] RadarrSettings settings)
        {
            try
            {


                var result = await RadarrApi.SystemStatus(settings.ApiKey, settings.FullUri);
                return result.version != null;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Radarr");
                return false;
            }
        }

        /// <summary>
        /// Checks if we can connect to Sonarr with the provided settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [HttpPost("sonarr")]
        public async Task<bool> Sonarr([FromBody] SonarrSettings settings)
        {
            try
            {

                var result = await SonarrApi.SystemStatus(settings.ApiKey, settings.FullUri);
                return result.version != null;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Sonarr");
                return false;
            }
        }

        /// <summary>
        /// Checks if we can connect to Sonarr with the provided settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [HttpPost("couchpotato")]
        public async Task<bool> CouchPotato([FromBody] CouchPotatoSettings settings)
        {
            try
            {
                var result = await CouchPotatoApi.Status(settings.FullUri, settings.ApiKey);
                return result?.success ?? false;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test CP");
                return false;
            }
        }
    }
}