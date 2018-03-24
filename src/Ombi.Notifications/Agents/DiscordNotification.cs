﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Discord;
using Ombi.Api.Discord.Models;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Interfaces;
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
                                   IMovieRequestRepository m, ITvRequestRepository t, ISettingsService<CustomizationSettings> s)
            : base(sn, r, m, t,s,log)
        {
            Api = api;
            Logger = log;
        }

        // constants I needed but could not find
        public const string IMDB_BASE_URL = "http://www.imdb.com/title/";
        public const string TVDB_BASE_URL = "https://www.thetvdb.com/?tab=series&id=";

        // if true mentionAlias will post the alias to discord and trigger an @mention if set up as <@id>.  
        // It will also use the username instead of alias when talking about a user. e.g. "Requested by username on 16 March"
        public bool MentionAlias { get; set; } = true;

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
            var parsed = await LoadTemplate(NotificationAgent.Discord, NotificationType.NewRequest, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.NewRequest} is disabled for {NotificationAgent.Discord}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };

            
            notification.Other.Add("image", parsed.Image);

            if (Customization.ApplicationUrl.HasValue()) notification.Other.Add("authorUrl", $"{Customization.ApplicationUrl}requests");

            if (model.RequestType == RequestType.Movie)
            {
                string userString = MovieRequest.RequestedUser.Alias;
                if (MentionAlias)
                {
                    notification.Other.Add("mention", userString);
                    userString = MovieRequest.RequestedUser.UserName;
                }
                notification.Other.Add("footer", $"Requested by {MovieRequest.RequestedUser}  on {MovieRequest.RequestedDate.ToLongDateString()}");
                notification.Other.Add("author", "🎬 New Movie Request!");
                notification.Other.Add("title", $"{MovieRequest.Title} ({MovieRequest.ReleaseDate.Year})");
                notification.Other.Add("description", MovieRequest.Overview);
                notification.Other.Add("titleUrl", $"{IMDB_BASE_URL}{MovieRequest.ImdbId}");
            }

            if (model.RequestType == RequestType.TvShow)
            {
                string userString = TvRequest.RequestedUser.Alias;
                if (MentionAlias)
                {
                    notification.Other.Add("mention", userString);
                    userString = TvRequest.RequestedUser.UserName;
                }
                notification.Other.Add("footer", $"Requested by {userString} on {TvRequest.RequestedDate.ToLongDateString()}");
                notification.Other.Add("author", "📺 New TV Request!");
                notification.Other.Add("title", $"{TvRequest.Title} ({TvRequest.ParentRequest.ReleaseDate.Year})");
                notification.Other.Add("description", TvRequest.ParentRequest.Overview);
                notification.Other.Add("titleUrl", $"{TVDB_BASE_URL}{TvRequest.ParentRequest.TvDbId}");
            }

            await Send(notification, settings);
        }

        protected override async Task NewIssue(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Discord, NotificationType.Issue, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.Issue} is disabled for {NotificationAgent.Discord}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            notification.Other.Add("author", "New Issue!");
            notification.Other.Add("description", notification.Message);
            await Send(notification, settings);
        }

        protected override async Task IssueComment(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Discord, NotificationType.IssueComment, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.IssueComment} is disabled for {NotificationAgent.Discord}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            notification.Other.Add("author", "New Comment on Issue!");
            notification.Other.Add("description", notification.Message);
            await Send(notification, settings);
        }

        protected override async Task IssueResolved(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Discord, NotificationType.IssueResolved, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.IssueResolved} is disabled for {NotificationAgent.Discord}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            notification.Other.Add("author", "Issue Resolved!");
            notification.Other.Add("description", notification.Message);
            await Send(notification, settings);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var user = string.Empty;
            var title = string.Empty;
            var image = string.Empty;
            if (model.RequestType == RequestType.Movie)
            {
                user = MovieRequest.RequestedUser.UserAlias;
                title = MovieRequest.Title;
                image = MovieRequest.PosterPath;
            }
            else
            {
                user = TvRequest.RequestedUser.UserAlias;
                title = TvRequest.ParentRequest.Title;
                image = TvRequest.ParentRequest.PosterPath;
            }
            var message = $"Hello! The user '{user}' has requested {title} but it could not be added. This has been added into the requests queue and will keep retrying";
            var notification = new NotificationMessage
            {
                Message = message
            };
            notification.Other.Add("image", image);
            notification.Other.Add("author", "Request Added to Queue!");
            notification.Other.Add("description", notification.Message);
            await Send(notification, settings);
        }

        protected override async Task RequestDeclined(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Discord, NotificationType.RequestDeclined, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.RequestDeclined} is disabled for {NotificationAgent.Discord}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            notification.Other.Add("author", "Request Declined.");
            notification.Other.Add("description", notification.Message);
            await Send(notification, settings);
        }

        protected override async Task RequestApproved(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Discord, NotificationType.RequestApproved, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.RequestApproved} is disabled for {NotificationAgent.Discord}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };

            notification.Other.Add("image", parsed.Image);
            notification.Other.Add("author", "Request Approved!");
            notification.Other.Add("description", notification.Message);
            await Send(notification, settings);
        }

        protected override async Task AvailableRequest(NotificationOptions model, DiscordNotificationSettings settings)
        {
            var parsed = await LoadTemplate(NotificationAgent.Discord, NotificationType.RequestAvailable, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {NotificationType.RequestAvailable} is disabled for {NotificationAgent.Discord}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            notification.Other.Add("image", parsed.Image);
            notification.Other.Add("author", "Request Available!");
            notification.Other.Add("description", notification.Message);
            await Send(notification, settings);
        }

        protected override async Task Send(NotificationMessage model, DiscordNotificationSettings settings)
        {
            try
            {
                var discordBody = new DiscordWebhookBody
                {
                    // content = model.Message,
                    username = settings.Username,
                };

                model.Other.TryGetValue("author", out var author);
                model.Other.TryGetValue("title", out var title);
                model.Other.TryGetValue("titleUrl", out var titleUrl);
                model.Other.TryGetValue("image", out var image);
                model.Other.TryGetValue("footer", out var footer);
                model.Other.TryGetValue("authorUrl", out var authorUrl);
                model.Other.TryGetValue("description", out var description);
                model.Other.TryGetValue("mention", out var mention);

                List<DiscordField> fields = new List<DiscordField>();
                if (MentionAlias && mention.HasValue())
                {
                    fields.Add
                    (
                        new DiscordField
                        {
                            name = "Honourable Mentions",
                            value = mention
                        }
                    );
                }

                discordBody.embeds = new List<DiscordEmbeds>
                {
                    new DiscordEmbeds
                    {
                        title = title,
                        url = titleUrl,
                        image = new DiscordImage
                        {
                            url = image
                        },
                        author = new DiscordAuthor
                        {
                            name = author,
                            url = authorUrl
                        },
                        description = description,
                        footer = new DiscordFooter
                        {
                            text = footer
                        },
                        fields = fields
                    }
                };
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
    }
}
