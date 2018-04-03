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

            MentionAlias = new Dictionary<NotificationType, bool>
            {
                {NotificationType.RequestAvailable, true},
                {NotificationType.RequestApproved, true},
                {NotificationType.RequestDeclined, true}
            };
            // Temporary defaults
        }

        public const string ImdbBaseUrl = "http://www.imdb.com/title/";
        public const string TvdbBaseUrl = "https://www.thetvdb.com/?tab=series&id=";

        // Whether or not to post the alias to discord (for each notification type) which trigger an @mention if alias is set up as <@id>.  
        public Dictionary<NotificationType, bool> MentionAlias;
        // Whether to use alias or username when referencing a user
        public bool UsingAliasAsMention = true;

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

            var author = new DiscordAuthor
            {
                name = parsed.Subject,
                icon_url = "https://i.imgur.com/EPuxVav.png"
            };
            if (authorUrl.HasValue())
                author.url = authorUrl;

            DiscordEmbeds embed = null;
            if (model.RequestType == RequestType.Movie)
            {
                embed = CreateDiscordEmbed(author, MovieRequest, NotificationType.NewRequest, parsed);
            }
            else if (model.RequestType == RequestType.TvShow)
            {
                embed = CreateDiscordEmbed(author, TvRequest, NotificationType.NewRequest, parsed);
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

            var author = new DiscordAuthor
            {
                name = parsed.Subject,
                icon_url = "https://i.imgur.com/i1X39I2.png"
            };

            DiscordEmbeds embed = null;
            if (model.RequestType == RequestType.Movie)
            {
                embed = CreateDiscordEmbed(author, MovieRequest, NotificationType.RequestDeclined, parsed);
            }
            else if (model.RequestType == RequestType.TvShow)
            {
                embed = CreateDiscordEmbed(author, TvRequest, NotificationType.RequestDeclined, parsed);
            }
            await Send(notification, settings, embed);
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

            var author = new DiscordAuthor
            {
                name = parsed.Subject,
                icon_url = "https://i.imgur.com/sodXDGW.png"
            };

            DiscordEmbeds embed = null;
            if (model.RequestType == RequestType.Movie)
            {
                embed = CreateDiscordEmbed(author, MovieRequest, NotificationType.RequestApproved, parsed);
            }
            else if (model.RequestType == RequestType.TvShow)
            {
                embed = CreateDiscordEmbed(author, TvRequest, NotificationType.RequestApproved, parsed);
            }
            await Send(notification, settings, embed);
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

            // TODO: implement plex / emby url
            string authorUrl = null;
            /*
            if (Customization.ApplicationUrl.HasValue())
                authorUrl = $"{Customization.ApplicationUrl}requests";
            */

            var author = new DiscordAuthor
            {
                name = parsed.Subject,
                url = authorUrl,
                icon_url = "https://i.imgur.com/k4bX9KM.png"
            };
            
            DiscordEmbeds embed = null;
            if (model.RequestType == RequestType.Movie)
            {
                embed = CreateDiscordEmbed(author, MovieRequest, NotificationType.RequestAvailable, parsed);
            }
            else if (model.RequestType == RequestType.TvShow)
            {
                embed = CreateDiscordEmbed(author, TvRequest, NotificationType.RequestAvailable, parsed);
            }

            await Send(notification, settings, embed);
        }

        protected async Task Send(NotificationMessage model, DiscordNotificationSettings settings, DiscordEmbeds embed)
        {
            try
            {
                var discordBody = new DiscordWebhookBody
                {
                    username = settings.Username,
                    embeds = new List<DiscordEmbeds>
                    {
                        embed
                    },
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

        private DiscordEmbeds CreateDiscordEmbed(DiscordAuthor author, BaseRequest req, NotificationType type, NotificationMessageContent content)
        {
            MentionAlias.TryGetValue(type, out var mentionUser);
            // Grab info specific to TV / Movie requests
            string overview = null;
            var releaseYear = 0;
            string titleUrl = null;
            if (req is ChildRequests tvReq)
            {
                overview = tvReq.ParentRequest.Overview;
                releaseYear = tvReq.ParentRequest.ReleaseDate.Year;
                titleUrl = $"{TvdbBaseUrl}{tvReq.ParentRequest.TvDbId}";
            }
            else if (req is MovieRequests movieReq)
            {
                overview = movieReq.Overview;
                releaseYear = movieReq.ReleaseDate.Year;
                titleUrl = $"{ImdbBaseUrl}{movieReq.ImdbId}";
            }

            // Compact embed
            // If the image url is invalid then Disscord will not send any message
            var validImage = false;
            Uri.TryCreate(content.Image, UriKind.Absolute, out var validUri);
            if (validUri != null)
            {
                validImage = true;
            }

            var image = new DiscordImage { url = content.Image };
            var thumbnail = new DiscordImage { url = content.Image };
            if (content.ShowCompact)
            {
                image = null;
                overview = null;
                content.Message = null;
            }            
            

            // Fields
            var fields = new List<DiscordField>();
            if (overview.HasValue())
            {
                fields.Add
                (
                    new DiscordField
                    {
                        name = "Overview",
                        value = overview
                    }
                );
            }
            // Field : mention user
            var alias = req.RequestedUser.Alias;
            if (UsingAliasAsMention && mentionUser && alias.HasValue())
            {
                fields.Add
                (
                    new DiscordField
                    {
                        name = "Honourable Mentions",
                        value = alias
                    }
                );
            }
            
            // Use appropriate user reference
            if (UsingAliasAsMention || !alias.HasValue())
                alias = req.RequestedUser.UserName;

            var footer = new DiscordFooter
            {
                text = $"Requested by {alias} on {req.RequestedDate.ToLongDateString()}"
            };

            var embed = new DiscordEmbeds
            {
                author = author,
                title = $"{req.Title} ({releaseYear})",
                url = titleUrl,
                description = content.Message,
                footer = footer,
                fields = fields
            };

            if (validImage)
            {
                embed.thumbnail = thumbnail;
                embed.image = image;
            }
            return embed;
        }
    }
}
