using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Discord;
using Ombi.Api.Discord.Models;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Notifications.Agents
{
    public class DiscordNotification : BaseNotification<DiscordNotificationSettings>, IDiscordNotification
    {
        public DiscordNotification(IDiscordApi api, ISettingsService<DiscordNotificationSettings> sn,
                                   ILogger<DiscordNotification> log, INotificationTemplatesRepository r,
                                   IMovieRequestRepository m, ITvRequestRepository t, ISettingsService<CustomizationSettings> s, IRepository<RequestSubscription> sub, IMusicRequestRepository music,
                                   IRepository<UserNotificationPreferences> userPref)
            : base(sn, r, m, t, s, log, sub, music, userPref)
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
                var b = settings.WebHookId;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        protected override async Task NewRequest(NotificationOptions model, DiscordNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.NewRequest);
        }

        protected override async Task NewIssue(NotificationOptions model, DiscordNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.Issue);
        }

        protected override async Task IssueComment(NotificationOptions model, DiscordNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.IssueComment);
        }

        protected override async Task IssueResolved(NotificationOptions model, DiscordNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.IssueResolved);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, DiscordNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.ItemAddedToFaultQueue);
        }

        protected override async Task RequestDeclined(NotificationOptions model, DiscordNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.RequestDeclined);
        }

        protected override async Task RequestApproved(NotificationOptions model, DiscordNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.RequestApproved);
        }

        protected override async Task AvailableRequest(NotificationOptions model, DiscordNotificationSettings settings)
        {
            await Run(model, settings, NotificationType.RequestAvailable);
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

                var fields = new List<DiscordField>();

                if (model.Data.TryGetValue("Alias", out var alias))
                {
                    if (alias.HasValue())
                    {
                        fields.Add(new DiscordField { name = "Requested By", value = alias, inline = true });
                    }
                }
                else
                {
                    if (model.Data.TryGetValue("RequestedUser", out var requestedUser))
                    {
                        if (requestedUser.HasValue())
                        {
                            fields.Add(new DiscordField { name = "Requested By", value = requestedUser, inline = true });
                        }
                    }
                }
                if (model.Data.TryGetValue("DenyReason", out var denyReason))
                {
                    if (denyReason.HasValue())
                    {
                        fields.Add(new DiscordField { name = "Denied Reason", value = denyReason, inline = true });
                    }
                }

                string color = null;
                if (model.Data.TryGetValue("RequestStatus", out var status))
                {
                    if (status.HasValue())
                    {
                        fields.Add(new DiscordField { name = "Status", value = status, inline = true });

                        color = status switch
                        {
                            "Available" => "51283",
                            "Denied" => "13959168",
                            "Processing Request" => "37354",
                            "Pending Approval" => "16754470",
                            _ => throw new ArgumentOutOfRangeException(nameof(color))
                        };
                    }
                }

                var author = new DiscordAuthor
                {
                    iconurl = settings.Icon.HasValue() ? settings.Icon : string.Empty
                };

                if (model.Data.TryGetValue("ApplicationUrl", out var appUrl))
                {
                    author.url = appUrl;
                }
                if (model.Data.TryGetValue("ApplicationName", out var appName))
                {
                    author.name = appName;
                }

                

                var embed = new DiscordEmbeds
                {
                    fields = fields,
                    author = author,
                    color = color
                };

                if (model.Data.TryGetValue("Title", out var title))
                {
                    embed.title = title;
                }
                if (model.Data.TryGetValue("Overview", out var overview))
                {
                    embed.description = overview;
                }
                string image;
                if (model.Other.TryGetValue("image", out image))
                {
                    embed.thumbnail = new DiscordImage { url = image };
                }

                if (model.Data.Any())
                {
                    discordBody.embeds = new List<DiscordEmbeds> { embed };
                }
                await Api.SendMessage(discordBody, settings.WebHookId, settings.Token);
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

        private async Task Run(NotificationOptions model, DiscordNotificationSettings settings, NotificationType type)
        {
            var parsed = await LoadTemplate(NotificationAgent.Discord, type, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {type} is disabled for {NotificationAgent.Discord}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
                Data = parsed.Data.ToDictionary(x => x.Key, x => x.Value)
            };
            notification.Other.Add("image", parsed.Image);
            await Send(notification, settings);
        }
    }
}
