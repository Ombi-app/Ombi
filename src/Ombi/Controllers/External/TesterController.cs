using System;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;
using Ombi.Core.Notifications;
using Ombi.Helpers;
using Ombi.Notifications.Agents;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models.Notifications;

namespace Ombi.Controllers.External
{
    /// <summary>
    /// The Tester Controller
    /// </summary>
    /// <seealso cref="Ombi.Controllers.BaseV1ApiController" />
    [Admin]
    [ApiV1]
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
        public TesterController(INotificationService service, IDiscordNotification notification, IEmailNotification emailN,
            IPushbulletNotification pushbullet, ISlackNotification slack)
        {
            Service = service;
            DiscordNotification = notification;
            EmailNotification = emailN;
            PushbulletNotification = pushbullet;
            SlackNotification = slack;
        }

        private INotificationService Service { get; }
        private IDiscordNotification DiscordNotification { get; }
        private IEmailNotification EmailNotification { get; }
        private IPushbulletNotification PushbulletNotification { get; }
        private ISlackNotification SlackNotification { get; }


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
        public bool Email([FromBody] EmailNotificationSettings settings)
        {
            settings.Enabled = true;
            var notificationModel = new NotificationOptions
            {
                NotificationType = NotificationType.Test,
                RequestId = -1 
            };
            EmailNotification.NotifyAsync(notificationModel, settings);

            return true;
        }
    }
}