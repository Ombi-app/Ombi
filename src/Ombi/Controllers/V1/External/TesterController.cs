﻿using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.CouchPotato;
using Ombi.Api.Emby;
using Ombi.Api.Jellyfin;
using Ombi.Api.Lidarr;
using Ombi.Api.Plex;
using Ombi.Api.Radarr;
using Ombi.Api.SickRage;
using Ombi.Api.Sonarr;
using Ombi.Api.Twilio;
using Ombi.Attributes;
using Ombi.Core.Authentication;
using Ombi.Core.Models;
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
using Ombi.Store.Entities;

namespace Ombi.Controllers.V1.External
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
            IPlexApi plex, IEmbyApiFactory emby, IRadarrV3Api radarr, ISonarrV3Api sonarr, ILogger<TesterController> log, IEmailProvider provider,
            ICouchPotatoApi cpApi, ITelegramNotification telegram, ISickRageApi srApi, INewsletterJob newsletter, ILegacyMobileNotification mobileNotification,
            ILidarrApi lidarrApi, IGotifyNotification gotifyNotification, IWhatsAppApi whatsAppApi, OmbiUserManager um, IWebhookNotification webhookNotification,
            IJellyfinApi jellyfinApi, IPrincipal user)
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
            GotifyNotification = gotifyNotification;
            WhatsAppApi = whatsAppApi;
            UserManager = um;
            WebhookNotification = webhookNotification;
            _jellyfinApi = jellyfinApi;
            UserPrinciple = user;
        }

        private INotificationService Service { get; }
        private IDiscordNotification DiscordNotification { get; }
        private IEmailNotification EmailNotification { get; }
        private IPushbulletNotification PushbulletNotification { get; }
        private ISlackNotification SlackNotification { get; }
        private IPushoverNotification PushoverNotification { get; }
        private IGotifyNotification GotifyNotification { get; }
        private IWebhookNotification WebhookNotification { get; }
        private IMattermostNotification MattermostNotification { get; }
        private IPlexApi PlexApi { get; }
        private IRadarrV3Api RadarrApi { get; }
        private IEmbyApiFactory EmbyApi { get; }
        private ISonarrV3Api SonarrApi { get; }
        private ICouchPotatoApi CouchPotatoApi { get; }
        private ILogger<TesterController> Log { get; }
        private IEmailProvider EmailProvider { get; }
        private ITelegramNotification TelegramNotification { get; }
        private ISickRageApi SickRageApi { get; }
        private INewsletterJob Newsletter { get; }
        private ILegacyMobileNotification MobileNotification { get; }
        private ILidarrApi LidarrApi { get; }
        private IWhatsAppApi WhatsAppApi { get; }
        private OmbiUserManager UserManager {get; }
        private readonly IJellyfinApi _jellyfinApi;
        private IPrincipal UserPrinciple { get; }

        /// <summary>
        /// Sends a test message to discord using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("discord")]
        public async Task<bool> Discord([FromBody] DiscordNotificationSettings settings)
        {
            try
            {
                settings.Enabled = true;
                await DiscordNotification.NotifyAsync(
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
        public async Task<bool> Pushbullet([FromBody] PushbulletSettings settings)
        {
            try
            {

                settings.Enabled = true;
                await PushbulletNotification.NotifyAsync(
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
        public async Task<bool> Pushover([FromBody] PushoverSettings settings)
        {
            try
            {
                settings.Enabled = true;
                await PushoverNotification.NotifyAsync(
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
        /// Sends a test message to Gotify using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("gotify")]
        public async Task<bool> Gotify([FromBody] GotifySettings settings)
        {
            try
            {
                settings.Enabled = true;
                await GotifyNotification.NotifyAsync(
                    new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

                return true;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Gotify");
                return false;
            }

        }

        /// <summary>
        /// Sends a test message to configured webhook using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("webhook")]
        public async Task<bool> Webhook([FromBody] WebhookSettings settings)
        {
            try
            {
                settings.Enabled = true;
                await WebhookNotification.NotifyAsync(
                    new NotificationOptions { NotificationType = NotificationType.Test, RequestId = -1 }, settings);

                return true;
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test your webhook");
                return false;
            }

        }

        /// <summary>
        /// Sends a test message to mattermost using the provided settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost("mattermost")]
        public async Task<bool> Mattermost([FromBody] MattermostNotificationSettings settings)
        {
            try
            {
                settings.Enabled = true;
                await MattermostNotification.NotifyAsync(
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
        public async Task<bool> Slack([FromBody] SlackNotificationSettings settings)
        {
            try
            {
                settings.Enabled = true;
                await SlackNotification.NotifyAsync(
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
                var currentUser = await GetCurrentUserAsync();

                if (!currentUser.Email.HasValue())
                {
                    throw new Exception($"User '{currentUser.UserName}' has no email address set on their user profile.");
                }

                var message = new NotificationMessage
                {
                    Message = "This is just a test! Success!",
                    Subject = $"Ombi: Test",
                    To = currentUser.Email,
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

        private async Task<OmbiUser> GetCurrentUserAsync()
        {
            return await UserManager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == UserPrinciple.Identity.Name.ToUpper());
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
                var client = await EmbyApi.CreateClient();
                var result = await client.GetUsers(settings.FullUri, settings.ApiKey);
                return result.Any();
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Emby");
                return false;
            }
        }

        /// <summary>
        ///  Checks if we can connect to Jellyfin with the provided settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [HttpPost("jellyfin")]
        public async Task<bool> Jellyfin([FromBody] JellyfinServers settings)
        {
            try
            {
                var result = await _jellyfinApi.GetUsers(settings.FullUri, settings.ApiKey);
                return result.Any();
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Jellyfin");
                return false;
            }
        }

        /// <summary>
        /// Checks if we can connect to Radarr with the provided settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [HttpPost("radarr")]
        public async Task<TesterResultModel> Radarr([FromBody] RadarrSettings settings)
        {
            try
            {


                var result = await RadarrApi.SystemStatus(settings.ApiKey, settings.FullUri);
                return new TesterResultModel
                {
                    IsValid = result.urlBase == settings.SubDir || string.IsNullOrEmpty(result.urlBase) && string.IsNullOrEmpty(settings.SubDir),
                    ExpectedSubDir = result.urlBase
                };
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Radarr");
                return new TesterResultModel { IsValid = false };
            }
        }

        /// <summary>
        /// Checks if we can connect to Sonarr with the provided settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [HttpPost("sonarr")]
        public async Task<TesterResultModel> Sonarr([FromBody] SonarrSettings settings)
        {
            try
            {
                if (string.IsNullOrEmpty(settings.ApiKey))
                {
                    return new TesterResultModel
                    {
                        IsValid = false,
                        AdditionalInformation = "NullApiKey"
                    };
                }
                if (string.IsNullOrEmpty(settings.Ip))
                {
                    return new TesterResultModel
                    {
                        IsValid = false,
                        AdditionalInformation = "NullIp"
                    };
                }
                if (settings.Port <= 0)
                {
                    return new TesterResultModel
                    {
                        IsValid = false,
                        AdditionalInformation = "BadPort"
                    };
                }

                var result = await SonarrApi.SystemStatus(settings.ApiKey, settings.FullUri);
                return new TesterResultModel
                {
                    IsValid = result.urlBase == settings.SubDir || string.IsNullOrEmpty(result.urlBase) && string.IsNullOrEmpty(settings.SubDir),
                    Version = result.version,
                    ExpectedSubDir = result.urlBase
                };
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Sonarr");
                return new TesterResultModel { IsValid = false };
            }
        }

        /// <summary>
        /// Checks if we can connect to CouchPotato with the provided settings
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
        public async Task<TesterResultModel> LidarrTest([FromBody] LidarrSettings settings)
        {
            try
            {
                var status = await LidarrApi.Status(settings.ApiKey, settings.FullUri);
                return new TesterResultModel
                {
                    IsValid = status?.urlBase == settings.SubDir || string.IsNullOrEmpty(status.urlBase) && string.IsNullOrEmpty(settings.SubDir),
                    ExpectedSubDir = status?.urlBase
                };
            }
            catch (Exception e)
            {
                Log.LogError(LoggingEvents.Api, e, "Could not test Lidarr");
                return new TesterResultModel { IsValid = false };
            }
        }

        [HttpPost("whatsapp")]
        public async Task<bool> WhatsAppTest([FromBody] WhatsAppSettingsViewModel settings)
        {
            try
            {

                var user = await UserManager.Users.Include(x => x.UserNotificationPreferences).FirstOrDefaultAsync(x => x.UserName == HttpContext.User.Identity.Name);


                var status = await WhatsAppApi.SendMessage(new WhatsAppModel {
                    From = settings.From,
                    Message = "This is a test from Ombi!",
                    To = user.UserNotificationPreferences.FirstOrDefault(x => x.Agent == NotificationAgent.WhatsApp).Value
                }, settings.AccountSid, settings.AuthToken);
                if (status.HasValue())
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
                Log.LogError(LoggingEvents.Api, e, "Could not test Lidarr");
                return false;
            }
        }
    }
}