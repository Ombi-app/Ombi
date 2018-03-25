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
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Store.Entities.Requests;

namespace Ombi.Notifications.Agents
{
    public class DiscordNotification : BaseNotification<DiscordNotificationSettings>, IDiscordNotification
    {
        public DiscordNotification(IDiscordApi api, ISettingsService<DiscordNotificationSettings> sn,
                                   ILogger<DiscordNotification> log, INotificationTemplatesRepository r,
                                   IMovieRequestRepository m, ITvRequestRepository t, ISettingsService<CustomizationSettings> s)
            : base(sn, r, m, t, s, log)
        {
            Api = api;
            Logger = log;
            ShowCompactEmbed = new Dictionary<NotificationType, bool>();
            // Temporary defaults
            ShowCompactEmbed.Add(NotificationType.RequestAvailable, true);
        }

        // constants I needed but could not find
        public const string IMDB_BASE_URL = "http://www.imdb.com/title/";
        public const string TVDB_BASE_URL = "https://www.thetvdb.com/?tab=series&id=";

        // if true mentionAlias will post the alias to discord and trigger a    n @mention if set up as <@id>.  
        // It will also use the username instead of alias when talking about a user. e.g. "Requested by username on 16 March"
        public bool MentionAlias { get; set; } = true;

        // Whether or not to show a compact embed notification (thumbnail + no description)
        Dictionary<NotificationType, bool> ShowCompactEmbed;

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

            string authorUrl = null;
            if (Customization.ApplicationUrl.HasValue())
                authorUrl = $"{Customization.ApplicationUrl}requests";

            ShowCompactEmbed.TryGetValue(NotificationType.NewRequest, out var compact);

            DiscordEmbed embed = null;
            if (model.RequestType == RequestType.Movie)
            {
                embed = createDiscordEmbed("🎬 New Movie Request!", authorUrl, parsed.Image, MovieRequest, compact);
            }
            else if (model.RequestType == RequestType.TvShow)
            {
                embed = createDiscordEmbed("📺 New TV Show Request!", authorUrl, parsed.Image, TvRequest, compact);
            }
            await Send(notification, settings, embed);
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

            // TODO implement plex / emby url
            string authorUrl = null;
            /*
            if (Customization.ApplicationUrl.HasValue())
                authorUrl = $"{Customization.ApplicationUrl}requests";
            
            */
            ShowCompactEmbed.TryGetValue(NotificationType.RequestAvailable, out var compact);

            DiscordEmbed embed = null;
            if (model.RequestType == RequestType.Movie)
            {
                embed = createDiscordEmbed("🎬 Requested Movie Available!", authorUrl, parsed.Image, MovieRequest, compact);
            }
            else if (model.RequestType == RequestType.TvShow)
            {
                embed = createDiscordEmbed("📺 Requested TV Show Available!", authorUrl, parsed.Image, TvRequest, compact);
            }

            await Send(notification, settings, embed);
        }

        protected async Task Send(NotificationMessage model, DiscordNotificationSettings settings, DiscordEmbed embed)
        {
            try
            {
                var discordBody = new DiscordWebhookBody
                {
                    username = settings.Username,
                };
                discordBody.embeds = new List<DiscordEmbed>
                {
                    embed
                };
                await Api.SendMessage(discordBody, settings.WebHookId, settings.Token);
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.DiscordNotification, e, "Failed to send Discord Notification");
            }
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

                model.Other.TryGetValue("image", out var image);
                discordBody.embeds = new List<DiscordEmbed>

                {
                    new DiscordEmbed
                    {
                        image = new DiscordImage
                        {
                            url = image
                        }
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

        private DiscordEmbed createDiscordEmbed(string authorName, string authorUrl, string imageUrl, MovieRequests req, bool compact)
        {
            DiscordAuthor author = null;
            if (authorName.HasValue())
            {
                author = new DiscordAuthor
                {
                    name = authorName,
                    url = authorUrl
                };
            }

            DiscordImage image = null;
            DiscordImage thumbnail = null;
            if (compact)
                thumbnail = new DiscordImage { url = imageUrl };
            else
                image = new DiscordImage { url = imageUrl };

            string description = null;
            if (!compact)
                description = MovieRequest.Overview;

            List<DiscordField> fields = new List<DiscordField>();
            string alias = MovieRequest.RequestedUser.Alias;
            if (MentionAlias)
            {
                fields.Add
                (
                    new DiscordField
                    {
                        name = "Honourable Mentions",
                        value = alias
                    }
                );
                alias = MovieRequest.RequestedUser.UserName;
            }

            DiscordFooter footer = new DiscordFooter
            {
                text = $"Requested by {alias}  on {MovieRequest.RequestedDate.ToLongDateString()}"
            };

            DiscordEmbed embed = new DiscordEmbed
            {
                author = author,
                title = $"{MovieRequest.Title} ({MovieRequest.ReleaseDate.Year})",
                url = $"{IMDB_BASE_URL}{MovieRequest.ImdbId}",
                thumbnail = thumbnail,
                image = image,
                description = description,
                footer = footer,
                fields = fields
            };
            return embed;
        }

        private DiscordEmbed createDiscordEmbed(string authorName, string authorUrl, string imageUrl, ChildRequests req, bool compact)
        {
            DiscordAuthor author = null;
            if (authorName.HasValue())
            {
                author = new DiscordAuthor
                {
                    name = authorName,
                    url = authorUrl
                };
            }

            DiscordImage image = null;
            DiscordImage thumbnail = null;
            if (compact)
                thumbnail = new DiscordImage { url = imageUrl };
            else
                image = new DiscordImage { url = imageUrl };

            string description = null;
            if (!compact)
                description = req.ParentRequest.Overview;

            List<DiscordField> fields = new List<DiscordField>();

            string alias = req.RequestedUser.Alias;
            if (MentionAlias)
            {
                fields.Add
                (
                    new DiscordField
                    {
                        name = "Honourable Mentions",
                        value = alias
                    }
                );
                alias = req.RequestedUser.UserName;
            }
            DiscordFooter footer = new DiscordFooter
            {
                text = $"Requested by {alias}  on {req.RequestedDate.ToLongDateString()}"
            };

            DiscordEmbed embed = new DiscordEmbed
            {
                author = author,
                title = $"{req.Title} ({req.ParentRequest.ReleaseDate.Year})",
                url = $"{TVDB_BASE_URL}{req.ParentRequest.TvDbId}",
                thumbnail = thumbnail,
                image = image,
                description = description,
                footer = footer,
                fields = fields
            };
            return embed;
        }
    }
}
