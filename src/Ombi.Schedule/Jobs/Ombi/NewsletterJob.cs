using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MailKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using Ombi.Api.CouchPotato.Models;
using Ombi.Api.Lidarr;
using Ombi.Api.Lidarr.Models;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Api.TvMaze;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Notifications;
using Ombi.Notifications.Models;
using Ombi.Notifications.Templates;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Org.BouncyCastle.Utilities.Collections;
using Quartz;
using ContentType = Ombi.Store.Entities.ContentType;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class NewsletterJob : HtmlTemplateGenerator, INewsletterJob
    {
        public NewsletterJob(IPlexContentRepository plex, IEmbyContentRepository emby, IJellyfinContentRepository jellyfin, IRepository<RecentlyAddedLog> addedLog,
            IMovieDbApi movieApi, ITvMazeApi tvApi, IEmailProvider email, ISettingsService<CustomizationSettings> custom,
            ISettingsService<EmailNotificationSettings> emailSettings, INotificationTemplatesRepository templateRepo,
            UserManager<OmbiUser> um, ISettingsService<NewsletterSettings> newsletter, ILogger<NewsletterJob> log,
            ILidarrApi lidarrApi, IExternalRepository<LidarrAlbumCache> albumCache, ISettingsService<LidarrSettings> lidarrSettings,
            ISettingsService<OmbiSettings> ombiSettings, ISettingsService<PlexSettings> plexSettings, ISettingsService<EmbySettings> embySettings, ISettingsService<JellyfinSettings> jellyfinSettings,
            IHubContext<NotificationHub> notification, IRefreshMetadata refreshMetadata)
        {
            _plex = plex;
            _emby = emby;
            _jellyfin = jellyfin;
            _recentlyAddedLog = addedLog;
            _movieApi = movieApi;
            _tvApi = tvApi;
            _email = email;
            _customizationSettings = custom;
            _templateRepo = templateRepo;
            _emailSettings = emailSettings;
            _newsletterSettings = newsletter;
            _userManager = um;
            _log = log;
            _lidarrApi = lidarrApi;
            _lidarrAlbumRepository = albumCache;
            _lidarrSettings = lidarrSettings;
            _ombiSettings = ombiSettings;
            _plexSettings = plexSettings;
            _embySettings = embySettings;
            _jellyfinSettings = jellyfinSettings;
            _notification = notification;
            _ombiSettings.ClearCache();
            _plexSettings.ClearCache();
            _emailSettings.ClearCache();
            _customizationSettings.ClearCache();
            _refreshMetadata = refreshMetadata;
        }

        private readonly IPlexContentRepository _plex;
        private readonly IEmbyContentRepository _emby;
        private readonly IJellyfinContentRepository _jellyfin;
        private readonly IRepository<RecentlyAddedLog> _recentlyAddedLog;
        private readonly IMovieDbApi _movieApi;
        private readonly ITvMazeApi _tvApi;
        private readonly IEmailProvider _email;
        private readonly ISettingsService<CustomizationSettings> _customizationSettings;
        private readonly INotificationTemplatesRepository _templateRepo;
        private readonly ISettingsService<EmailNotificationSettings> _emailSettings;
        private readonly ISettingsService<NewsletterSettings> _newsletterSettings;
        private readonly ISettingsService<OmbiSettings> _ombiSettings;
        private readonly UserManager<OmbiUser> _userManager;
        private readonly ILogger _log;
        private readonly ILidarrApi _lidarrApi;
        private readonly IExternalRepository<LidarrAlbumCache> _lidarrAlbumRepository;
        private readonly ISettingsService<LidarrSettings> _lidarrSettings;
        private readonly ISettingsService<PlexSettings> _plexSettings;
        private readonly ISettingsService<EmbySettings> _embySettings;
        private readonly ISettingsService<JellyfinSettings> _jellyfinSettings;
        private readonly IHubContext<NotificationHub> _notification;
        private readonly IRefreshMetadata _refreshMetadata;

        public async Task Start(NewsletterSettings settings, bool test)
        {
            if (!settings.Enabled)
            {
                return;
            }
            var template = await _templateRepo.GetTemplate(NotificationAgent.Email, NotificationType.Newsletter);
            if (!template.Enabled)
            {
                return;
            }

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Newsletter Started");
            var emailSettings = await _emailSettings.GetSettingsAsync();
            if (!ValidateConfiguration(emailSettings))
            {
                await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                    .SendAsync(NotificationHub.NotificationEvent, "Newsletter Email Settings Not Configured");
                return;
            }

            try
            {


                var customization = await _customizationSettings.GetSettingsAsync();
                var plexContent = (IQueryable<IMediaServerContent>)_plex.GetAll().Include(x => x.Episodes).AsNoTracking();
                var embyContent = (IQueryable<IMediaServerContent>)_emby.GetAll().Include(x => x.Episodes).AsNoTracking();
                var jellyfinContent = (IQueryable<IMediaServerContent>)_jellyfin.GetAll().Include(x => x.Episodes).AsNoTracking();

                // MOVIES
                var plexContentMoviesToSend = await GetMoviesContent(plexContent, RecentlyAddedType.Plex);
                var embyContentMoviesToSend = await GetMoviesContent(embyContent, RecentlyAddedType.Emby);
                var jellyfinContentMoviesToSend = await GetMoviesContent(jellyfinContent, RecentlyAddedType.Jellyfin);

                // MUSIC
                var lidarrContent = _lidarrAlbumRepository.GetAll().AsNoTracking().ToList().Where(x => x.FullyAvailable);
                var addedLog = _recentlyAddedLog.GetAll().ToList();
                HashSet<string> addedAlbumLogIds;
                GetRecentlyAddedMoviesData(addedLog, out addedAlbumLogIds);

                // EPISODES
                var addedPlexEpisodesLogIds =
                addedLog.Where(x => x.Type == RecentlyAddedType.Plex && x.ContentType == ContentType.Episode);
                var addedEmbyEpisodesLogIds =
                    addedLog.Where(x => x.Type == RecentlyAddedType.Emby && x.ContentType == ContentType.Episode);
                var addedJellyfinEpisodesLogIds =
                    addedLog.Where(x => x.Type == RecentlyAddedType.Jellyfin && x.ContentType == ContentType.Episode);


                // Filter out the ones that we haven't sent yet
                var lidarrContentAlbumsToSend = lidarrContent.Where(x => !addedAlbumLogIds.Contains(x.ForeignAlbumId)).ToHashSet();
                _log.LogInformation("Albums to send: {0}", lidarrContentAlbumsToSend.Count());

                var plexEpisodesToSend =
                    FilterEpisodes(_plex.GetAllEpisodes().Include(x => x.Series).AsNoTracking(), addedPlexEpisodesLogIds);
                var embyEpisodesToSend = FilterEpisodes(_emby.GetAllEpisodes().Include(x => x.Series).AsNoTracking(),
                    addedEmbyEpisodesLogIds);
                var jellyfinEpisodesToSend = FilterEpisodes(_jellyfin.GetAllEpisodes().Include(x => x.Series).AsNoTracking(),
                    addedJellyfinEpisodesLogIds);

                _log.LogInformation("Plex Episodes to send: {0}", plexEpisodesToSend.Count());
                _log.LogInformation("Emby Episodes to send: {0}", embyEpisodesToSend.Count());
                _log.LogInformation("Jellyfin Episodes to send: {0}", jellyfinEpisodesToSend.Count());
                var plexSettings = await _plexSettings.GetSettingsAsync();
                var embySettings = await _embySettings.GetSettingsAsync();
                var jellyfinSettings = await _jellyfinSettings.GetSettingsAsync();
                var body = string.Empty;
                if (test)
                {
                    var plexm = plexContent.Where(x => x.Type == MediaType.Movie).OrderByDescending(x => x.AddedAt).Take(10);
                    var embym = embyContent.Where(x => x.Type == MediaType.Movie).OrderByDescending(x => x.AddedAt).Take(10);
                    var jellyfinm = jellyfinContent.Where(x => x.Type == MediaType.Movie).OrderByDescending(x => x.AddedAt).Take(10);
                    var plext = _plex.GetAllEpisodes().Include(x => x.Series).OrderByDescending(x => x.Series.AddedAt).Take(10).ToHashSet();
                    var embyt = _emby.GetAllEpisodes().Include(x => x.Series).OrderByDescending(x => x.Series.AddedAt).Take(10).ToHashSet();
                    var jellyfint = _jellyfin.GetAllEpisodes().Include(x => x.Series).OrderByDescending(x => x.Series.AddedAt).Take(10).ToHashSet();
                    var lidarr = lidarrContent.OrderByDescending(x => x.AddedAt).Take(10).ToHashSet();

                    var moviesProviders =  new List<IQueryable<IMediaServerContent>>() {
                        plexm,
                        embym,
                        jellyfinm
                    };
                    var seriesProviders =  new List<IEnumerable<IMediaServerEpisode>>() {
                        plext,
                        embyt,
                        jellyfint
                    };
                    body = await BuildHtml(moviesProviders, seriesProviders, lidarr, settings, embySettings, jellyfinSettings, plexSettings);
                }
                else
                {
                    var moviesProviders =  new List<IQueryable<IMediaServerContent>>() {
                        plexContentMoviesToSend.AsQueryable(),
                        embyContentMoviesToSend.AsQueryable(),
                        jellyfinContentMoviesToSend.AsQueryable()
                    };
                    var seriesProviders =  new List<IEnumerable<IMediaServerEpisode>>() {
                        plexEpisodesToSend,
                        embyEpisodesToSend,
                        jellyfinEpisodesToSend
                    };

                    body = await BuildHtml(moviesProviders, seriesProviders, lidarrContentAlbumsToSend, settings, embySettings, jellyfinSettings, plexSettings);
                    if (body.IsNullOrEmpty())
                    {
                        return;
                    }
                }

                if (!test)
                {
                    var users = new List<OmbiUser>();
                    foreach (var emails in settings.ExternalEmails)
                    {
                        users.Add(new OmbiUser
                        {
                            UserName = emails,
                            Email = emails
                        });
                    }

                    // Get the users to send it to
                    users.AddRange(await _userManager.GetUsersInRoleAsync(OmbiRoles.ReceivesNewsletter));
                    if (!users.Any())
                    {
                        return;
                    }

                    var messageContent = ParseTemplate(template, customization);
                    var email = new NewsletterTemplate();

                    foreach (var user in users.DistinctBy(x => x.Email))
                    {                        // Get the users to send it to
                        if (user.Email.IsNullOrEmpty())
                        {
                            continue;
                        }

                        var url = GenerateUnsubscribeLink(customization.ApplicationUrl, user.Id);
                        var html = email.LoadTemplate(messageContent.Subject, messageContent.Message, body, customization.Logo, url);

                        var bodyBuilder = new BodyBuilder
                        {
                            HtmlBody = html,
                        };

                        var message = new MimeMessage
                        {
                            Body = bodyBuilder.ToMessageBody(),
                            Subject = messageContent.Subject
                        };

                        // Send the message to the user
                        message.To.Add(new MailboxAddress(user.Email.Trim(), user.Email.Trim()));

                        // Send the email
                        await _email.Send(message, emailSettings);
                    }

                    // Now add all of this to the Recently Added log
                    var recentlyAddedLog = new HashSet<RecentlyAddedLog>();
                    foreach (var p in plexContentMoviesToSend)
                    {
                        recentlyAddedLog.Add(new RecentlyAddedLog
                        {
                            AddedAt = DateTime.Now,
                            Type = RecentlyAddedType.Plex,
                            ContentType = ContentType.Parent,
                            ContentId = StringHelper.IntParseLinq(p.TheMovieDbId),
                        });

                    }

                    foreach (var p in plexEpisodesToSend)
                    {
                        recentlyAddedLog.Add(new RecentlyAddedLog
                        {
                            AddedAt = DateTime.Now,
                            Type = RecentlyAddedType.Plex,
                            ContentType = ContentType.Episode,
                            ContentId = StringHelper.IntParseLinq(p.Series.TvDbId),
                            EpisodeNumber = p.EpisodeNumber,
                            SeasonNumber = p.SeasonNumber
                        });
                    }
                    foreach (var e in embyContentMoviesToSend)
                    {
                        if (e.Type == MediaType.Movie)
                        {
                            recentlyAddedLog.Add(new RecentlyAddedLog
                            {
                                AddedAt = DateTime.Now,
                                Type = RecentlyAddedType.Emby,
                                ContentType = ContentType.Parent,
                                ContentId = StringHelper.IntParseLinq(e.TheMovieDbId),
                            });
                        }
                    }

                    foreach (var p in embyEpisodesToSend)
                    {
                        recentlyAddedLog.Add(new RecentlyAddedLog
                        {
                            AddedAt = DateTime.Now,
                            Type = RecentlyAddedType.Emby,
                            ContentType = ContentType.Episode,
                            ContentId = StringHelper.IntParseLinq(p.Series.TvDbId),
                            EpisodeNumber = p.EpisodeNumber,
                            SeasonNumber = p.SeasonNumber
                        });
                    }

                    foreach (var e in jellyfinContentMoviesToSend)
                    {
                        if (e.Type == MediaType.Movie)
                        {
                            recentlyAddedLog.Add(new RecentlyAddedLog
                            {
                                AddedAt = DateTime.Now,
                                Type = RecentlyAddedType.Jellyfin,
                                ContentType = ContentType.Parent,
                                ContentId = StringHelper.IntParseLinq(e.TheMovieDbId),
                            });
                        }
                    }

                    foreach (var p in jellyfinEpisodesToSend)
                    {
                        recentlyAddedLog.Add(new RecentlyAddedLog
                        {
                            AddedAt = DateTime.Now,
                            Type = RecentlyAddedType.Jellyfin,
                            ContentType = ContentType.Episode,
                            ContentId = StringHelper.IntParseLinq(p.Series.TvDbId),
                            EpisodeNumber = p.EpisodeNumber,
                            SeasonNumber = p.SeasonNumber
                        });
                    }

                    await _recentlyAddedLog.AddRange(recentlyAddedLog);
                }
                else
                {
                    var admins = await _userManager.GetUsersInRoleAsync(OmbiRoles.Admin);
                    foreach (var a in admins)
                    {
                        if (a.Email.IsNullOrEmpty())
                        {
                            continue;
                        }

                        var unsubscribeLink = GenerateUnsubscribeLink(customization.ApplicationUrl, a.Id);

                        var messageContent = ParseTemplate(template, customization);

                        var email = new NewsletterTemplate();

                        var html = email.LoadTemplate(messageContent.Subject, messageContent.Message, body, customization.Logo, unsubscribeLink);

                        await _email.Send(
                            new NotificationMessage { Message = html, Subject = messageContent.Subject, To = a.Email },
                            emailSettings);
                    }
                }

            }
            catch (Exception e)
            {
                await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                    .SendAsync(NotificationHub.NotificationEvent, "Newsletter Failed");
                _log.LogError(e, "Error when attempting to create newsletter");
                throw;
            }

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Newsletter Finished");
        }

        private void GetRecentlyAddedMoviesData(List<RecentlyAddedLog> addedLog, out HashSet<string> addedAlbumLogIds)
        {
            var lidarrParent = addedLog.Where(x => x.Type == RecentlyAddedType.Lidarr && x.ContentType == ContentType.Album);
            addedAlbumLogIds = lidarrParent != null && lidarrParent.Any() ? (lidarrParent?.Select(x => x.AlbumId)?.ToHashSet() ?? new HashSet<string>()) : new HashSet<string>();
        }

        private async Task<HashSet<IMediaServerContent>> GetMoviesContent(IQueryable<IMediaServerContent> content, RecentlyAddedType recentlyAddedType)
        {
            var localDataset = content.Where(x => x.Type == MediaType.Movie && !string.IsNullOrEmpty(x.TheMovieDbId)).ToHashSet();
            // Filter out the ones that we haven't sent yet
            var addedLog = _recentlyAddedLog.GetAll().ToList();
            var plexParent = addedLog.Where(x => x.Type == recentlyAddedType
               && x.ContentType == ContentType.Parent).ToList();
            var addedPlexMovieLogIds = plexParent != null && plexParent.Any() ? (plexParent?.Select(x => x.ContentId)?.ToHashSet() ?? new HashSet<int>()) : new HashSet<int>();
            var plexContentMoviesToSend = localDataset.Where(x => !addedPlexMovieLogIds.Contains(StringHelper.IntParseLinq(x.TheMovieDbId))).ToHashSet();
            _log.LogInformation("Movies to send: {0}", plexContentMoviesToSend.Count());

            // Find the movies that do not yet have MovieDbIds
            var needsMovieDbPlex = content.Where(x => x.Type == MediaType.Movie && !string.IsNullOrEmpty(x.TheMovieDbId)).ToHashSet();
            var newPlexMovies = await GetMoviesWithoutId(addedPlexMovieLogIds, needsMovieDbPlex);
            plexContentMoviesToSend = plexContentMoviesToSend.Union(newPlexMovies).ToHashSet();

            return plexContentMoviesToSend.DistinctBy(x => x.Id).ToHashSet();

        }

        public static string GenerateUnsubscribeLink(string applicationUrl, string id)
        {
            if (!applicationUrl.HasValue() || !id.HasValue())
            {
                return string.Empty;
            }

            if (!applicationUrl.EndsWith('/'))
            {
                applicationUrl += '/';
            }
            var b = new UriBuilder($"{applicationUrl}unsubscribe/{id}");
            return b.ToString();
        }

        private async Task<HashSet<IMediaServerContent>> GetMoviesWithoutId(HashSet<int> addedMovieLogIds, HashSet<IMediaServerContent> needsMovieDbPlex)
        {
            foreach (var movie in needsMovieDbPlex)
            {
                var id = await _refreshMetadata.GetTheMovieDbId(false, true, null, movie.ImdbId, movie.Title, true);
                movie.TheMovieDbId = id.ToString();
            }

            var result = needsMovieDbPlex.Where(x => x.HasTheMovieDb && !addedMovieLogIds.Contains(StringHelper.IntParseLinq(x.TheMovieDbId)));
            await UpdateTheMovieDbId(result);
            // Filter them out now
            return result.ToHashSet();
        }

        private async Task UpdateTheMovieDbId(IEnumerable<IMediaServerContent> content)
        {
            foreach (var movie in content)
            {
                if (!movie.HasTheMovieDb)
                {
                    continue;
                }
                var entity = await _plex.Find(movie.Id);
                if (entity == null)
                {
                    return;
                }
                entity.TheMovieDbId = movie.TheMovieDbId;
                _plex.UpdateWithoutSave(entity);
            }
            await _plex.SaveChangesAsync();
        }

        public async Task Execute(IJobExecutionContext job)
        {
            var newsletterSettings = await _newsletterSettings.GetSettingsAsync();
            await Start(newsletterSettings, false);
        }

        private HashSet<IMediaServerEpisode> FilterEpisodes(IEnumerable<IMediaServerEpisode> source, IEnumerable<RecentlyAddedLog> recentlyAdded)
        {
            var itemsToReturn = new HashSet<IMediaServerEpisode>();
            foreach (var ep in source.Where(x => x.Series.HasTvDb))
            {
                var tvDbId = StringHelper.IntParseLinq(ep.Series.TvDbId);
                if (recentlyAdded.Any(x => x.ContentId == tvDbId && x.EpisodeNumber == ep.EpisodeNumber && x.SeasonNumber == ep.SeasonNumber))
                {
                    continue;
                }

                itemsToReturn.Add(ep);
            }

            return itemsToReturn;
        }

        private NotificationMessageContent ParseTemplate(NotificationTemplates template, CustomizationSettings settings)
        {
            var resolver = new NotificationMessageResolver();
            var curlys = new NotificationMessageCurlys();

            curlys.SetupNewsletter(settings);

            return resolver.ParseMessage(template, curlys);
        }

        private async Task<string> BuildHtml(ICollection<IQueryable<IMediaServerContent>> contentToSend,
            ICollection<IEnumerable<IMediaServerEpisode>> episodes, HashSet<LidarrAlbumCache> albums, NewsletterSettings settings, EmbySettings embySettings, JellyfinSettings jellyfinSettings,
            PlexSettings plexSettings)
        {
            var ombiSettings = await _ombiSettings.GetSettingsAsync();
            var sb = new StringBuilder();

            if (!settings.DisableMovies)
            {
                sb.Append("<h1 style=\"text-align: center; max-width: 1042px;\">New Movies</h1><br /><br />");
                sb.Append(
                "<table class=\"movies-table\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; \">");
                sb.Append("<tr>");
                sb.Append("<td style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 14px; vertical-align: top; \">");
                sb.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; \">");
                sb.Append("<tr>");
                foreach (var mediaServerContent in contentToSend)
                {
                    await ProcessMovies(mediaServerContent, sb, ombiSettings.DefaultLanguageCode, /*plexSettings.Servers?.FirstOrDefault()?.ServerHostname ?? */ string.Empty);
                }
                sb.Append("</tr>");
                sb.Append("</table>");
                sb.Append("</td>");
                sb.Append("</tr>");
                sb.Append("</table>");
            }

            if (!settings.DisableTv)
            {
                sb.Append("<br /><br /><h1 style=\"text-align: center; max-width: 1042px;\">New TV</h1><br /><br />");
                sb.Append(
                "<table class=\"tv-table\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; \">");
                sb.Append("<tr>");
                sb.Append("<td style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 14px; vertical-align: top; \">");
                sb.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; \">");
                sb.Append("<tr>");
                foreach (var mediaServerContent in episodes)
                {
                    await ProcessTv(mediaServerContent, sb, ombiSettings.DefaultLanguageCode, /* plexSettings.Servers.FirstOrDefault()?.ServerHostname ?? */ string.Empty);
                }
                sb.Append("</tr>");
                sb.Append("</table>");
                sb.Append("</td>");
                sb.Append("</tr>");
                sb.Append("</table>");
            }


            if (albums.Any() && !settings.DisableMusic)
            {
                sb.Append("<h1 style=\"text-align: center; max-width: 1042px;\">New Albums</h1><br /><br />");
                sb.Append(
                    "<table class=\"movies-table\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; \">");
                sb.Append("<tr>");
                sb.Append("<td style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 14px; vertical-align: top; \">");
                sb.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; \">");
                sb.Append("<tr>");
                await ProcessAlbums(albums, sb);
                sb.Append("</tr>");
                sb.Append("</table>");
                sb.Append("</td>");
                sb.Append("</tr>");
                sb.Append("</table>");
            }

            return sb.ToString();
        }

        private async Task ProcessMovies(IQueryable<IMediaServerContent> plexContentToSend, StringBuilder sb, string defaultLanguageCode, string mediaServerUrl)
        {
            int count = 0;
            var ordered = plexContentToSend.OrderByDescending(x => x.AddedAt);
            foreach (var content in ordered)
            {
                int.TryParse(content.TheMovieDbId, out var movieDbId);
                if (movieDbId <= 0)
                {
                    continue;
                }
                var info = await _movieApi.GetMovieInformationWithExtraInfo(movieDbId, defaultLanguageCode);
                var mediaurl = PlexHelper.BuildPlexMediaUrl(content.Url, mediaServerUrl);
                if (info == null)
                {
                    continue;
                }
                try
                {
                    CreateMovieHtmlContent(sb, info, mediaurl);
                    count += 1;
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error when Processing Movies {0}", info.Title);
                }
                finally
                {
                    EndLoopHtml(sb);
                }

                if (count == 2)
                {
                    count = 0;
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                }
            }
        }
        private async Task ProcessAlbums(HashSet<LidarrAlbumCache> albumsToSend, StringBuilder sb)
        {
            var settings = await _lidarrSettings.GetSettingsAsync();
            int count = 0;
            var ordered = albumsToSend.OrderByDescending(x => x.AddedAt);
            foreach (var content in ordered)
            {
                var info = await _lidarrApi.GetAlbumByForeignId(content.ForeignAlbumId, settings.ApiKey, settings.FullUri);
                if (info == null)
                {
                    continue;
                }
                try
                {
                    CreateAlbumHtmlContent(sb, info);
                    count += 1;
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error when Processing Lidarr Album {0}", info.title);
                }
                finally
                {
                    EndLoopHtml(sb);
                }

                if (count == 2)
                {
                    count = 0;
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                }
            }
        }

        private void CreateMovieHtmlContent(StringBuilder sb, MovieResponseDto info, string mediaurl)
        {
            AddBackgroundInsideTable(sb, $"https://image.tmdb.org/t/p/w1280/{info.BackdropPath}");
            AddPosterInsideTable(sb, $"https://image.tmdb.org/t/p/original{info.PosterPath}");

            AddMediaServerUrl(sb, mediaurl, $"https://image.tmdb.org/t/p/original{info.PosterPath}");
            AddInfoTable(sb);

            var releaseDate = string.Empty;
            try
            {
                releaseDate = $"({DateTime.Parse(info.ReleaseDate).Year})";
            }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
            catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
            {
                // Swallow, couldn't parse the date
            }

            AddTitle(sb, $"https://www.imdb.com/title/{info.ImdbId}/", $"{info.Title} {releaseDate}");

            var summary = info.Overview;
            if (summary.Length > 280)
            {
                summary = summary.Remove(280);
                summary = summary + "...</p>";
            }
            AddParagraph(sb, summary);

            if (info.Genres.Any())
            {
                AddGenres(sb,
                    $"Genres: {string.Join(", ", info.Genres.Select(x => x.Name.ToString()).ToArray())}");
            }
        }

        private void CreateAlbumHtmlContent(StringBuilder sb, AlbumLookup info)
        {
            var cover = info.images
                .FirstOrDefault(x => x.coverType.Equals("cover", StringComparison.InvariantCultureIgnoreCase))?.url;
            if (cover.IsNullOrEmpty())
            {
                cover = info.remoteCover;
            }
            AddBackgroundInsideTable(sb, cover);
            var disk = info.images
                .FirstOrDefault(x => x.coverType.Equals("disc", StringComparison.InvariantCultureIgnoreCase))?.url;
            if (disk.IsNullOrEmpty())
            {
                disk = info.remoteCover;
            }
            AddPosterInsideTable(sb, disk);

            AddMediaServerUrl(sb, string.Empty, string.Empty);
            AddInfoTable(sb);

            var releaseDate = $"({info.releaseDate.Year})";

            AddTitle(sb, string.Empty, $"{info.title} {releaseDate}");

            var summary = info.artist?.artistName ?? string.Empty;
            if (summary.Length > 280)
            {
                summary = summary.Remove(280);
                summary = summary + "...</p>";
            }
            AddParagraph(sb, summary);

            AddGenres(sb, $"Type: {info.albumType}");
        }

        private async Task ProcessTv(IEnumerable<IMediaServerEpisode> episodes, StringBuilder sb, string languageCode, string serverHostname)
        {
            var series = new List<IMediaServerContent>();
            foreach (var episode in episodes)
            {
                var existingSeries = episode.SeriesIsIn(series);
                if (existingSeries != null)
                {
                    if (!episode.IsIn(existingSeries))
                    {
                        existingSeries.Episodes.Add(episode);
                    }
                }
                else
                {
                    episode.Series.Episodes = new List<IMediaServerEpisode> { episode };
                    series.Add(episode.Series);
                }
            }

            int count = 0;
            var orderedTv = series.OrderByDescending(x => x.AddedAt);
            foreach (var t in orderedTv)
            {
                if (!t.HasTvDb)
                {
                    // We may need to use themoviedb for the imdbid or their own id to get info
                    if (t.HasTheMovieDb)
                    {
                        int.TryParse(t.TheMovieDbId, out var movieId);
                        var externals = await _movieApi.GetTvExternals(movieId);
                        if (externals == null || externals.tvdb_id <= 0)
                        {
                            continue;
                        }
                        t.TvDbId = externals.tvdb_id.ToString();
                    }
                    // WE could check the below but we need to get the moviedb and then perform the above, let the metadata job figure this out.
                    //else if(t.HasImdb)
                    //{
                    //    // Check the imdbid
                    //    var externals = await _movieApi.Find(t.ImdbId, ExternalSource.imdb_id);
                    //    if (externals?.tv_results == null || externals.tv_results.Length <= 0)
                    //    {
                    //        continue;
                    //    }
                    //    t.TvDbId = externals.tv_results.FirstOrDefault()..ToString();
                    //}

                }

                int.TryParse(t.TvDbId, out var tvdbId);
                var info = await _tvApi.ShowLookupByTheTvDbId(tvdbId);
                if (info == null)
                {
                    continue;
                }

                try
                {
                    var banner = info.image?.original;
                    if (!string.IsNullOrEmpty(banner))
                    {
                        banner = banner.ToHttpsUrl(); // Always use the Https banners
                    }

                    var tvInfo = await _movieApi.GetTVInfo(t.TheMovieDbId, languageCode);
                    if (tvInfo != null && tvInfo.backdrop_path.HasValue())
                    {

                        AddBackgroundInsideTable(sb, $"https://image.tmdb.org/t/p/w500{tvInfo.backdrop_path}");
                    }
                    else
                    {
                        AddBackgroundInsideTable(sb, $"https://image.tmdb.org/t/p/w1280/");
                    }
                    AddPosterInsideTable(sb, banner);
                    AddMediaServerUrl(sb, PlexHelper.BuildPlexMediaUrl(t.Url, serverHostname), banner);
                    AddInfoTable(sb);

                    AddTvTitle(sb, info, tvInfo);

                    // Group by the season number
                    var results = t.Episodes.GroupBy(p => p.SeasonNumber,
                        (key, g) => new
                        {
                            SeasonNumber = key,
                            Episodes = g.ToList(),
                            EpisodeAirDate = tvInfo?.seasons?.Where(x => x.season_number == key)?.Select(x => x.air_date).FirstOrDefault()
                        }
                    );

                    // Group the episodes
                    var finalsb = new StringBuilder();
                    foreach (var epInformation in results.OrderBy(x => x.SeasonNumber))
                    {
                        var orderedEpisodes = epInformation.Episodes.OrderBy(x => x.EpisodeNumber).ToList();
                        var episodeString = StringHelper.BuildEpisodeList(orderedEpisodes.Select(x => x.EpisodeNumber));
                        var episodeAirDate = epInformation.EpisodeAirDate;
                        finalsb.Append($"Season: {epInformation.SeasonNumber} - Episodes: {episodeString} {episodeAirDate}");
                        finalsb.Append("<br />");
                    }

                    AddTvEpisodesSummaryGenres(sb, finalsb.ToString(), tvInfo);

                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error when processing Plex TV {0}", t.Title);
                }
                finally
                {
                    EndLoopHtml(sb);
                    count += 1;
                }

                if (count == 2)
                {
                    count = 0;
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                }
            }
        }

        private void AddTvTitle(StringBuilder sb, Api.TvMaze.Models.TvMazeShow info, TvInfo tvInfo)
        {
            var title = "";
            if (!String.IsNullOrEmpty(info.premiered) && info.premiered.Length > 4)
            {
                title = $"{tvInfo.name} ({info.premiered.Remove(4)})";
            }
            else
            {
                title = $"{tvInfo.name}";
            }
            AddTitle(sb, $"https://www.imdb.com/title/{info.externals.imdb}/", title);
        }

        private void AddTvEpisodesSummaryGenres(StringBuilder sb, string episodes, TvInfo tvInfo)
        {
            var summary = tvInfo.overview;
            if (summary.Length > 280)
            {
                summary = summary.Remove(280);
                summary = summary + "...</p>";
            }
            AddTvParagraph(sb, episodes, summary);

            if (tvInfo.genres.Any())
            {
                AddGenres(sb, $"Genres: {string.Join(", ", tvInfo.genres.Select(x => x.name.ToString()).ToArray())}");
            }
        }

        private void EndLoopHtml(StringBuilder sb)
        {
            //NOTE: BR have to be in TD's as per html spec or it will be put outside of the table...
            //Source: http://stackoverflow.com/questions/6588638/phantom-br-tag-rendered-by-browsers-prior-to-table-tag
            sb.Append("</table>");
            sb.Append("</td>");
            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("</td>");
            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("</td>");
        }

        protected bool ValidateConfiguration(EmailNotificationSettings settings)
        {
            if (!settings.Enabled)
            {
                return false;
            }
            if (settings.Authentication)
            {
                if (string.IsNullOrEmpty(settings.Username) || string.IsNullOrEmpty(settings.Password))
                {
                    return false;
                }
            }
            if (string.IsNullOrEmpty(settings.Host) || string.IsNullOrEmpty(settings.AdminEmail) || string.IsNullOrEmpty(settings.Port.ToString()))
            {
                return false;
            }

            return true;
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                //_newsletterSettings?.Dispose();
                //_customizationSettings?.Dispose();
                //_emailSettings.Dispose();
                _templateRepo?.Dispose();
                _userManager?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
