using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Discord;
using Ombi.Api.Discord.Models;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Interfaces;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Repository;

namespace Ombi.Notifications.Agents
{
    public class DiscordNotification : BaseNotification<DiscordNotificationSettings>, IDiscordNotification
    {
        public DiscordNotification(IDiscordApi api, ISettingsService<DiscordNotificationSettings> sn, ILogger<DiscordNotification> log, INotificationTemplatesRepository r) : base(sn, r)
        {
            Api = api;
            Logger = log;
        }

        public override string NotificationName => "DiscordNotification";

        private IDiscordApi Api { get; }
        private ILogger<DiscordNotification> Logger { get; }

        protected override bool ValidateConfiguration(DiscordNotificationSettings settings)
        {
            if (!settings.Enabled)
            {
                return false;
            }
            if (string.IsNullOrEmpty(settings.WebhookUrl))
            {
                return false;
            }
            try
            {
                var a = settings.Token;
                var b = settings.WebookId;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        protected override async Task NewRequest(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var template = await TemplateRepository.GetTemplate(NotificationAgent.Email, NotificationType.NewRequest);

            var notification = new NotificationMessage
            {
                Message = template.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task Issue(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var template = await TemplateRepository.GetTemplate(NotificationAgent.Email, NotificationType.Issue);

            var notification = new NotificationMessage
            {
                Message = template.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var message = $"Hello! The user '{model.RequestedUser}' has requested {model.Title} but it could not be added. This has been added into the requests queue and will keep retrying";
            var notification = new NotificationMessage
            {
                Message = message
            };
            notification.Other.Add("image", model.ImgSrc);
            await Send(notification, settings);
        }

        protected override async Task RequestDeclined(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var template = await TemplateRepository.GetTemplate(NotificationAgent.Email, NotificationType.RequestDeclined);

            var notification = new NotificationMessage
            {
                Message = template.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task RequestApproved(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var template = await TemplateRepository.GetTemplate(NotificationAgent.Email, NotificationType.RequestApproved);

            var notification = new NotificationMessage
            {
                Message = template.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task AvailableRequest(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var template = await TemplateRepository.GetTemplate(NotificationAgent.Email, NotificationType.RequestAvailable);

            var notification = new NotificationMessage
            {
                Message = template.Message,
            };
            await Send(notification, settings);
        }

        protected override async Task Send(NotificationMessage model, DiscordNotificationSettings settings)
        {
            try
            {
                var discordBody = new DiscordWebhookBody
                {
                    content = model.Message,
                    username = settings.Username,
                };

                string image;
                if (model.Other.TryGetValue("image", out image))
                {
                    discordBody.embeds = new List<DiscordEmbeds>
                    {
                        new DiscordEmbeds
                        {
                            image = new DiscordImage
                            {
                                url = image
                            }
                        }
                    };
                }
                
                await Api.SendMessage(discordBody, settings.WebookId, settings.Token);
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.DiscordNotification, e, "Failed to send Discord Notification");
            }
        }

        protected override async Task Test(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var message = $"This is a test from Ombi, if you can see this then we have successfully pushed a notification!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }
    }
}
