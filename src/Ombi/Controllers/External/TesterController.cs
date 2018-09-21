using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ombi.Api.CouchPotato;
using Ombi.Api.Emby;
using Ombi.Api.Lidarr;
using Ombi.Api.Plex;
using Ombi.Api.Radarr;
using Ombi.Api.SickRage;
using Ombi.Api.Sonarr;
using Ombi.Attributes;
using Ombi.Core.Models.UI;
using Ombi.Core.Notifications;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Models;
using Ombi.Notifications;
using Ombi.Notifications.Agents;
using Ombi.Notifications.Models;
using Ombi.Schedule.Jobs.Ombi;
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
        public TesterController(INotificationService service, IDiscordNotification notification, IEmailNotification emailN,
            IPushbulletNotification pushbullet, ISlackNotification slack, IPushoverNotification po, IMattermostNotification mm,
            IPlexApi plex, IEmbyApi emby, IRadarrApi radarr, ISonarrApi sonarr, ILogger<TesterController> log, IEmailProvider provider,
            ICouchPotatoApi cpApi, ITelegramNotification telegram, ISickRageApi srApi, INewsletterJob newsletter, IMobileNotification mobileNotification,
            ILidarrApi lidarrApi)
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
            TelegramNotification = telegram;
            SickRageApi = srApi;
            Newsletter = newsletter;
            MobileNotification = mobileNotification;
            LidarrApi = lidarrApi;
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
        private ITelegramNotification TelegramNotification { get; }
        private ISickRageApi SickRageApi { get; }
        private INewsletterJob Newsletter { get; }
        private IMobileNotification MobileNotification { get; }
        private ILidarrApi LidarrApi { get; }


        /// <summary>
        /// Sends a test message to discord using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("discord")]
        public bool Discord([FromBody] DiscordNotificationSettings settings)
        {
            try
            {
                settings.Enabled = true;
                DiscordNotification.NotifyAsync(
                    new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

                return true;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Discord");
                return false;
            }
        }

        /// <summary>
        /// Sends a test message to Pushbullet using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("pushbullet")]
        public bool Pushbullet([FromBody] PushbulletSettings settings)
        {
            try
            {

                settings.Enabled = true;
                PushbulletNotification.NotifyAsync(
                    new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

                return true;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Pushbullet");
                return false;
            }
        }

        /// <summary>
        /// Sends a test message to Pushover using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("pushover")]
        public bool Pushover([FromBody] PushoverSettings settings)
        {
            try
            {
                settings.Enabled = true;
                PushoverNotification.NotifyAsync(
                    new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

                return true;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Pushover");
                return false;
            }

        }

        /// <summary>
        /// Sends a test message to mattermost using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("mattermost")]
        public bool Mattermost([FromBody] MattermostNotificationSettings settings)
        {
            try
            {
                settings.Enabled = true;
                MattermostNotification.NotifyAsync(
                    new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

                return true;

            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Mattermost");
                return false;
            }

        }


        /// <summary>
        /// Sends a test message to Slack using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("slack")]
        public bool Slack([FromBody] SlackNotificationSettings settings)
        {
            try
            {
                settings.Enabled = true;
                SlackNotification.NotifyAsync(
                    new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

                return true;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Slack");
                return false;
            }
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

        /// <summary>
        /// Sends a test message to Telegram using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("telegram")]
        public async Task<bool> Telegram([FromBody] TelegramSettings settings)
        {
            try
            {
                settings.Enabled = true;
                await TelegramNotification.NotifyAsync(new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

                return true;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Telegram");
                return false;
            }
        }

        /// <summary>
        /// Sends a test message to Slack using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("sickrage")]
        public async Task<bool> SickRage([FromBody] SickRageSettings settings)
        {
            try
            {
                settings.Enabled = true;
                var result = await SickRageApi.Ping(settings.ApiKey, settings.FullUri);
                return result?.data?.pid != null;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test SickRage");
                return false;
            }
        }

        [HttpPost("newsletter")]
        public async Task<bool> NewsletterTest([FromBody] NewsletterNotificationViewModel settings)
        {
            try
            {
                settings.Enabled = true;
                await Newsletter.Start(settings, true);
                return true;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Newsletter");
                return false;
            }
        }

        [HttpPost("mobile")]
        public async Task<bool> MobileNotificationTest([FromBody] MobileNotificationTestViewModel settings)
        {
            try
            {
                await MobileNotification.NotifyAsync(new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1, UserId = settings.UserId }, settings.Settings);

                return true;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Mobile Notifications");
                return false;
            }
        }

        [HttpPost("lidarr")]
        public async Task<bool> LidarrTest([FromBody] LidarrSettings settings)
        {
            try
            {
                var status = await LidarrApi.Status(settings.ApiKey, settings.FullUri);
                if (status != null & status?.version.HasValue() ?? false)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Mobile Notifications");
                return false;
            }
        }
    }
}