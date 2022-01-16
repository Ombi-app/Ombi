using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
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

        private readonly IMediaServerContentRepository<PlexServerContent> _plex;
        private readonly IMediaServerContentRepository<EmbyContent> _emby;
        private readonly IMediaServerContentRepository<JellyfinContent> _jellyfin;
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

                var plexSettings = await _plexSettings.GetSettingsAsync();
                var embySettings = await _embySettings.GetSettingsAsync();
                var jellyfinSettings = await _jellyfinSettings.GetSettingsAsync();

                var customization = await _customizationSettings.GetSettingsAsync();

                var moviesContents = new List<IMediaServerContent>();
                var seriesContents = new List<IMediaServerEpisode>();
                if (plexSettings.Enable)
                {
                    moviesContents.AddRange(await GetMoviesContent(_plex, test));
                    seriesContents.AddRange(GetSeriesContent(_plex, test));
                }
                if (embySettings.Enable)
                {
                    moviesContents.AddRange(await GetMoviesContent(_emby, test));
                    seriesContents.AddRange(GetSeriesContent(_emby, test));
                }
                if (jellyfinSettings.Enable)
                {
                    moviesContents.AddRange(await GetMoviesContent(_jellyfin, test));
                    seriesContents.AddRange(GetSeriesContent(_jellyfin, test));
                }

                var albumsContents = GetMusicContent(_lidarrAlbumRepository, test);

                var body = await BuildHtml(moviesContents, seriesContents, albumsContents, settings);

                if (body.IsNullOrEmpty())
                {
                    return;
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
                    AddToRecentlyAddedLog(moviesContents, recentlyAddedLog);
                    AddToRecentlyAddedLog(seriesContents, recentlyAddedLog);
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

        private void AddToRecentlyAddedLog(ICollection<IMediaServerContent> moviesContents,
                                           HashSet<RecentlyAddedLog> recentlyAddedLog)
        {
            foreach (var p in moviesContents)
            {
                recentlyAddedLog.Add(new RecentlyAddedLog
                {
                    AddedAt = DateTime.Now,
                    Type = p.RecentlyAddedType,
                    ContentType = ContentType.Parent,
                    ContentId = StringHelper.IntParseLinq(p.TheMovieDbId),
                });
            }
        }
        private void AddToRecentlyAddedLog(ICollection<IMediaServerEpisode> episodes,
                                           HashSet<RecentlyAddedLog> recentlyAddedLog)
        {
            foreach (var p in episodes)
            {
                recentlyAddedLog.Add(new RecentlyAddedLog
                {
                    AddedAt = DateTime.Now,
                    Type = p.Series.RecentlyAddedType, 
                    ContentType = ContentType.Episode,
                    ContentId = StringHelper.IntParseLinq(p.Series.TvDbId),
                    EpisodeNumber = p.EpisodeNumber,
                    SeasonNumber = p.SeasonNumber
                });
            }
        }

        private void GetRecentlyAddedMoviesData(List<RecentlyAddedLog> addedLog, out HashSet<string> addedAlbumLogIds)
        {
            var lidarrParent = addedLog.Where(x => x.Type == RecentlyAddedType.Lidarr && x.ContentType == ContentType.Album);
            addedAlbumLogIds = lidarrParent != null && lidarrParent.Any() ? (lidarrParent?.Select(x => x.AlbumId)?.ToHashSet() ?? new HashSet<string>()) : new HashSet<string>();
        }

        private async Task<HashSet<IMediaServerContent>> GetMoviesContent<T>(IMediaServerContentRepository<T> repository, bool test) where T : class, IMediaServerContent
        {
            IQueryable<IMediaServerContent> content = repository.GetAll().Include(x => x.Episodes).AsNoTracking().Where(x => x.Type == MediaType.Movie).OrderByDescending(x => x.AddedAt);
            var localDataset = content.Where(x => x.Type == MediaType.Movie && !string.IsNullOrEmpty(x.TheMovieDbId)).ToHashSet();

            HashSet<IMediaServerContent> moviesToSend;
            if (test)
            {
                moviesToSend = content.Take(10).ToHashSet();
            }
            else
            {
                // Filter out the ones that we haven't sent yet
                var parent = _recentlyAddedLog.GetAll().Where(x => x.Type == repository.RecentlyAddedType
                   && x.ContentType == ContentType.Parent).ToList();
                var addedMovieLogIds = parent != null && parent.Any() ? (parent?.Select(x => x.ContentId)?.ToHashSet() ?? new HashSet<int>()) : new HashSet<int>();
                moviesToSend = localDataset.Where(x => !addedMovieLogIds.Contains(StringHelper.IntParseLinq(x.TheMovieDbId))).ToHashSet();
                _log.LogInformation("Movies to send: {0}", moviesToSend.Count());

                // Find the movies that do not yet have MovieDbIds
                var needsMovieDb = content.Where(x => x.Type == MediaType.Movie && !string.IsNullOrEmpty(x.TheMovieDbId)).ToHashSet();
                var newMovies = await GetMoviesWithoutId(addedMovieLogIds, needsMovieDb, repository);
                moviesToSend = moviesToSend.Union(newMovies).ToHashSet();
            }

            _log.LogInformation("Movies to send: {0}", moviesToSend.Count());
            return moviesToSend.DistinctBy(x => x.Id).ToHashSet();
        }

        private HashSet<IMediaServerEpisode> GetSeriesContent<T>(IMediaServerContentRepository<T> repository, bool test) where T : class, IMediaServerContent
        {
            var content = repository.GetAllEpisodes().Include(x => x.Series).OrderByDescending(x => x.Series.AddedAt).AsNoTracking();
            
            HashSet<IMediaServerEpisode> episodesToSend;
            if (test)
            {
                var count = repository.GetAllEpisodes().Count();
                _log.LogCritical($"Episodes test mode  {count}");
                _log.LogCritical(nameof(repository));
                episodesToSend = content.Take(10).ToHashSet();
            }
            else
            {
                // Filter out the ones that we haven't sent yet
                var addedEpisodesLogIds =
                _recentlyAddedLog.GetAll().Where(x => x.Type == repository.RecentlyAddedType && x.ContentType == ContentType.Episode);
                episodesToSend =
                    FilterEpisodes(content, addedEpisodesLogIds);
            }

            _log.LogInformation("Episodes to send: {0}", episodesToSend.Count());
            return episodesToSend;

        }
        private HashSet<LidarrAlbumCache> GetMusicContent(IExternalRepository<LidarrAlbumCache> repository, bool test)
        {

            var lidarrContent = repository.GetAll().AsNoTracking().ToList().Where(x => x.FullyAvailable);

            HashSet<LidarrAlbumCache> albumsToSend;
            if (test)
            {
                albumsToSend = lidarrContent.OrderByDescending(x => x.AddedAt).Take(10).ToHashSet();
            }
            else
            {
                // Filter out the ones that we haven't sent yet
                var addedLog = _recentlyAddedLog.GetAll().ToList();
                HashSet<string> addedAlbumLogIds;
                GetRecentlyAddedMoviesData(addedLog, out addedAlbumLogIds);
                albumsToSend = lidarrContent.Where(x => !addedAlbumLogIds.Contains(x.ForeignAlbumId)).ToHashSet();
            }
            _log.LogInformation("Albums to send: {0}", albumsToSend.Count());
            return albumsToSend;

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

        private async Task<HashSet<IMediaServerContent>> GetMoviesWithoutId<T>(HashSet<int> addedMovieLogIds, HashSet<IMediaServerContent> needsMovieDb, IMediaServerContentRepository<T> repository) where T : class, IMediaServerContent
        {
            foreach (var movie in needsMovieDb)
            {
                var id = await _refreshMetadata.GetTheMovieDbId(false, true, null, movie.ImdbId, movie.Title, true);
                movie.TheMovieDbId = id.ToString();
            }

            var result = needsMovieDb.Where(x => x.HasTheMovieDb && !addedMovieLogIds.Contains(StringHelper.IntParseLinq(x.TheMovieDbId)));
            await UpdateTheMovieDbId(result, repository);
            // Filter them out now
            return result.ToHashSet();
        }

        private async Task UpdateTheMovieDbId<T>(IEnumerable<IMediaServerContent> content, IMediaServerContentRepository<T> repository) where T : class, IMediaServerContent
        {
            foreach (var movie in content)
            {
                if (!movie.HasTheMovieDb)
                {
                    continue;
                }
                var entity = await repository.Find(movie.Id);
                if (entity == null)
                {
                    return;
                }
                entity.TheMovieDbId = movie.TheMovieDbId;
                repository.UpdateWithoutSave(entity);
            }
            await repository.SaveChangesAsync();
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

        private async Task<string> BuildHtml(ICollection<IMediaServerContent> movies,
            IEnumerable<IMediaServerEpisode> episodes, HashSet<LidarrAlbumCache> albums, NewsletterSettings settings)
        {
            var ombiSettings = await _ombiSettings.GetSettingsAsync();
            sb = new StringBuilder();

            if (!settings.DisableMovies)
            {
                sb.Append("<h1 style=\"text-align: center; max-width: 1042px;\">New Movies</h1><br /><br />");
                sb.Append(
                "<table class=\"movies-table\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; \">");
                sb.Append("<tr>");
                sb.Append("<td style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 14px; vertical-align: top; \">");
                sb.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; \">");
                sb.Append("<tr>");
                await ProcessMovies(movies, ombiSettings.DefaultLanguageCode);
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
                await ProcessTv(episodes, ombiSettings.DefaultLanguageCode);
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
                await ProcessAlbums(albums);
                sb.Append("</tr>");
                sb.Append("</table>");
                sb.Append("</td>");
                sb.Append("</tr>");
                sb.Append("</table>");
            }

            return sb.ToString();
        }

        private async Task ProcessMovies(ICollection<IMediaServerContent> plexContentToSend, string defaultLanguageCode)
        {
            int count = 0;
            var ordered = plexContentToSend;
            foreach (var content in ordered)
            {
                int.TryParse(content.TheMovieDbId, out var movieDbId);
                if (movieDbId <= 0)
                {
                    continue;
                }
                var info = await _movieApi.GetMovieInformationWithExtraInfo(movieDbId, defaultLanguageCode);
                var mediaurl = content.Url;
                if (info == null)
                {
                    continue;
                }
                try
                {
                    CreateMovieHtmlContent(info, mediaurl);
                    count += 1;
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error when Processing Movies {0}", info.Title);
                }
                finally
                {
                    EndLoopHtml();
                }

                if (count == 2)
                {
                    count = 0;
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                }
            }
        }
        private async Task ProcessAlbums(HashSet<LidarrAlbumCache> albumsToSend)
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
                    CreateAlbumHtmlContent(info);
                    count += 1;
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error when Processing Lidarr Album {0}", info.title);
                }
                finally
                {
                    EndLoopHtml();
                }

                if (count == 2)
                {
                    count = 0;
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                }
            }
        }

        private void CreateMovieHtmlContent(MovieResponseDto info, string mediaurl)
        {
            AddBackgroundInsideTable($"https://image.tmdb.org/t/p/w1280/{info.BackdropPath}");
            AddPosterInsideTable($"https://image.tmdb.org/t/p/original{info.PosterPath}");

            AddMediaServerUrl(mediaurl, $"https://image.tmdb.org/t/p/original{info.PosterPath}");
            AddInfoTable();

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

            AddTitle($"https://www.imdb.com/title/{info.ImdbId}/", $"{info.Title} {releaseDate}");

            var summary = info.Overview;
            if (summary.Length > 280)
            {
                summary = summary.Remove(280);
                summary = summary + "...</p>";
            }
            AddParagraph(summary);

            if (info.Genres.Any())
            {
                AddGenres($"Genres: {string.Join(", ", info.Genres.Select(x => x.Name.ToString()).ToArray())}");
            }
        }

        private void CreateAlbumHtmlContent(AlbumLookup info)
        {
            var cover = info.images
                .FirstOrDefault(x => x.coverType.Equals("cover", StringComparison.InvariantCultureIgnoreCase))?.url;
            if (cover.IsNullOrEmpty())
            {
                cover = info.remoteCover;
            }
            AddBackgroundInsideTable(cover);
            var disk = info.images
                .FirstOrDefault(x => x.coverType.Equals("disc", StringComparison.InvariantCultureIgnoreCase))?.url;
            if (disk.IsNullOrEmpty())
            {
                disk = info.remoteCover;
            }
            AddPosterInsideTable(disk);

            AddMediaServerUrl(string.Empty, string.Empty);
            AddInfoTable();

            var releaseDate = $"({info.releaseDate.Year})";

            AddTitle(string.Empty, $"{info.title} {releaseDate}");

            var summary = info.artist?.artistName ?? string.Empty;
            if (summary.Length > 280)
            {
                summary = summary.Remove(280);
                summary = summary + "...</p>";
            }
            AddParagraph(summary);

            AddGenres($"Type: {info.albumType}");
        }

        private async Task ProcessTv(IEnumerable<IMediaServerEpisode> episodes, string languageCode)
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

                        AddBackgroundInsideTable($"https://image.tmdb.org/t/p/w500{tvInfo.backdrop_path}");
                    }
                    else
                    {
                        AddBackgroundInsideTable($"https://image.tmdb.org/t/p/w1280/");
                    }
                    AddPosterInsideTable(banner);
                    AddMediaServerUrl(t.Url, banner);
                    AddInfoTable();

                    AddTvTitle(info, tvInfo);

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

                    AddTvEpisodesSummaryGenres(finalsb.ToString(), tvInfo);

                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error when processing Plex TV {0}", t.Title);
                }
                finally
                {
                    EndLoopHtml();
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

        private void AddTvTitle(Api.TvMaze.Models.TvMazeShow info, TvInfo tvInfo)
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
            AddTitle($"https://www.imdb.com/title/{info.externals.imdb}/", title);
        }

        private void AddTvEpisodesSummaryGenres(string episodes, TvInfo tvInfo)
        {
            var summary = tvInfo.overview;
            if (summary.Length > 280)
            {
                summary = summary.Remove(280);
                summary = summary + "...</p>";
            }
            AddTvParagraph(episodes, summary);

            if (tvInfo.genres.Any())
            {
                AddGenres($"Genres: {string.Join(", ", tvInfo.genres.Select(x => x.name.ToString()).ToArray())}");
            }
        }

        private void EndLoopHtml()
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
