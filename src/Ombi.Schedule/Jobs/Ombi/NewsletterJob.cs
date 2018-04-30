using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MailKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Api.TvMaze;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications;
using Ombi.Notifications.Models;
using Ombi.Notifications.Templates;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class NewsletterJob : HtmlTemplateGenerator, INewsletterJob
    {
        public NewsletterJob(IPlexContentRepository plex, IEmbyContentRepository emby, IRepository<RecentlyAddedLog> addedLog,
            IMovieDbApi movieApi, ITvMazeApi tvApi, IEmailProvider email, ISettingsService<CustomizationSettings> custom,
            ISettingsService<EmailNotificationSettings> emailSettings, INotificationTemplatesRepository templateRepo,
            UserManager<OmbiUser> um, ISettingsService<NewsletterSettings> newsletter, ILogger<NewsletterJob> log)
        {
            _plex = plex;
            _emby = emby;
            _recentlyAddedLog = addedLog;
            _movieApi = movieApi;
            _tvApi = tvApi;
            _email = email;
            _customizationSettings = custom;
            _templateRepo = templateRepo;
            _emailSettings = emailSettings;
            _newsletterSettings = newsletter;
            _userManager = um;
            _emailSettings.ClearCache();
            _customizationSettings.ClearCache();
            _newsletterSettings.ClearCache();
            _log = log;
        }

        private readonly IPlexContentRepository _plex;
        private readonly IEmbyContentRepository _emby;
        private readonly IRepository<RecentlyAddedLog> _recentlyAddedLog;
        private readonly IMovieDbApi _movieApi;
        private readonly ITvMazeApi _tvApi;
        private readonly IEmailProvider _email;
        private readonly ISettingsService<CustomizationSettings> _customizationSettings;
        private readonly INotificationTemplatesRepository _templateRepo;
        private readonly ISettingsService<EmailNotificationSettings> _emailSettings;
        private readonly ISettingsService<NewsletterSettings> _newsletterSettings;
        private readonly UserManager<OmbiUser> _userManager;
        private readonly ILogger _log;

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

            var emailSettings = await _emailSettings.GetSettingsAsync();
            if (!ValidateConfiguration(emailSettings))
            {
                return;
            }

            try
            {


                var customization = await _customizationSettings.GetSettingsAsync();
                // Get the Content
                var plexContent = _plex.GetAll().Include(x => x.Episodes).AsNoTracking();
                var embyContent = _emby.GetAll().Include(x => x.Episodes).AsNoTracking();

                var addedLog = _recentlyAddedLog.GetAll();
                var addedPlexMovieLogIds = addedLog.Where(x => x.Type == RecentlyAddedType.Plex && x.ContentType == ContentType.Parent).Select(x => x.ContentId);
                var addedEmbyMoviesLogIds = addedLog.Where(x => x.Type == RecentlyAddedType.Emby && x.ContentType == ContentType.Parent).Select(x => x.ContentId);

                var addedPlexEpisodesLogIds =
                addedLog.Where(x => x.Type == RecentlyAddedType.Plex && x.ContentType == ContentType.Episode);
                var addedEmbyEpisodesLogIds =
                    addedLog.Where(x => x.Type == RecentlyAddedType.Emby && x.ContentType == ContentType.Episode);

                // Filter out the ones that we haven't sent yet
                var plexContentMoviesToSend = plexContent.Where(x => x.Type == PlexMediaTypeEntity.Movie && x.HasTheMovieDb && !addedPlexMovieLogIds.Contains(StringHelper.IntParseLinq(x.TheMovieDbId)));
                var embyContentMoviesToSend = embyContent.Where(x => x.Type == EmbyMediaType.Movie && x.HasTheMovieDb && !addedEmbyMoviesLogIds.Contains(StringHelper.IntParseLinq(x.TheMovieDbId)));
                _log.LogInformation("Plex Movies to send: {0}", plexContentMoviesToSend.Count());
                _log.LogInformation("Emby Movies to send: {0}", embyContentMoviesToSend.Count());

                var plexEpisodesToSend =
                    FilterPlexEpisodes(_plex.GetAllEpisodes().Include(x => x.Series).Where(x => x.Series.HasTvDb).AsNoTracking(), addedPlexEpisodesLogIds);
                var embyEpisodesToSend = FilterEmbyEpisodes(_emby.GetAllEpisodes().Include(x => x.Series).Where(x => x.Series.HasTvDb).AsNoTracking(),
                    addedEmbyEpisodesLogIds);

                _log.LogInformation("Plex Episodes to send: {0}", plexEpisodesToSend.Count());
                _log.LogInformation("Emby Episodes to send: {0}", embyEpisodesToSend.Count());
                var body = string.Empty;
                if (test)
                {
                    var plexm = plexContent.Where(x => x.Type == PlexMediaTypeEntity.Movie).OrderByDescending(x => x.AddedAt).Take(10);
                    var embym = embyContent.Where(x => x.Type == EmbyMediaType.Movie ).OrderByDescending(x => x.AddedAt).Take(10);
                    var plext = _plex.GetAllEpisodes().Include(x => x.Series).OrderByDescending(x => x.Series.AddedAt).Take(10).ToHashSet();
                    var embyt = _emby.GetAllEpisodes().Include(x => x.Series).OrderByDescending(x => x.AddedAt).Take(10).ToHashSet();
                    body = await BuildHtml(plexm, embym, plext, embyt, settings);
                }
                else
                {
                    body = await BuildHtml(plexContentMoviesToSend, embyContentMoviesToSend, plexEpisodesToSend, embyEpisodesToSend, settings);
                    if (body.IsNullOrEmpty())
                    {
                        return;
                    }
                }

                if (!test)
                {
                    // Get the users to send it to
                    var users = await _userManager.GetUsersInRoleAsync(OmbiRoles.RecievesNewsletter);
                    if (!users.Any())
                    {
                        return;
                    }

                    foreach (var emails in settings.ExternalEmails)
                    {
                        users.Add(new OmbiUser
                        {
                            UserName = emails,
                            Email = emails
                        });
                    }
                    var emailTasks = new List<Task>();
                    foreach (var user in users)
                    {
                        // Get the users to send it to
                        if (user.Email.IsNullOrEmpty())
                        {
                            continue;
                        }

                        var messageContent = ParseTemplate(template, customization, user);
                        var email = new NewsletterTemplate();

                        var html = email.LoadTemplate(messageContent.Subject, messageContent.Message, body, customization.Logo);

                        emailTasks.Add(_email.Send(
                            new NotificationMessage { Message = html, Subject = messageContent.Subject, To = user.Email },
                            emailSettings));
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
                        if (e.Type == EmbyMediaType.Movie)
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
                    await _recentlyAddedLog.AddRange(recentlyAddedLog);
                    await Task.WhenAll(emailTasks.ToArray());
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
                        var messageContent = ParseTemplate(template, customization, a);

                        var email = new NewsletterTemplate();

                        var html = email.LoadTemplate(messageContent.Subject, messageContent.Message, body, customization.Logo);

                        await _email.Send(
                            new NotificationMessage { Message = html, Subject = messageContent.Subject, To = a.Email },
                            emailSettings);
                    }
                }

            }
            catch (Exception e)
            {
                _log.LogError(e, "Error when attempting to create newsletter");
                throw;
            }
        }

        public async Task Start()
        {
            var newsletterSettings = await _newsletterSettings.GetSettingsAsync();
            await Start(newsletterSettings, false);
        }

        private HashSet<PlexEpisode> FilterPlexEpisodes(IEnumerable<PlexEpisode> source, IQueryable<RecentlyAddedLog> recentlyAdded)
        {
            var itemsToReturn = new HashSet<PlexEpisode>();
            foreach (var ep in source)
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

        private HashSet<EmbyEpisode> FilterEmbyEpisodes(IEnumerable<EmbyEpisode> source, IQueryable<RecentlyAddedLog> recentlyAdded)
        {
            var itemsToReturn = new HashSet<EmbyEpisode>();
            foreach (var ep in source)
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

        private NotificationMessageContent ParseTemplate(NotificationTemplates template, CustomizationSettings settings, OmbiUser username)
        {
            var resolver = new NotificationMessageResolver();
            var curlys = new NotificationMessageCurlys();

            curlys.SetupNewsletter(settings, username);

            return resolver.ParseMessage(template, curlys);
        }

        private async Task<string> BuildHtml(IQueryable<PlexServerContent> plexContentToSend, IQueryable<EmbyContent> embyContentToSend, HashSet<PlexEpisode> plexEpisodes, HashSet<EmbyEpisode> embyEp, NewsletterSettings settings)
        {
            var sb = new StringBuilder();

            var plexMovies = plexContentToSend.Where(x => x.Type == PlexMediaTypeEntity.Movie);
            var embyMovies = embyContentToSend.Where(x => x.Type == EmbyMediaType.Movie);
            if ((plexMovies.Any() || embyMovies.Any()) && !settings.DisableMovies)
            {
                sb.Append("<h1 style=\"text-align: center;\">New Movies</h1><br /><br />");
                sb.Append(
                "<table class=\"movies-table\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100 %; \">");
                sb.Append("<tr>");
                sb.Append("<td style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 14px; vertical-align: top; \">");
                sb.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100 %; \">");
                sb.Append("<tr>");
                await ProcessPlexMovies(plexMovies, sb);
                await ProcessEmbyMovies(embyMovies, sb);
                sb.Append("</tr>");
                sb.Append("</table>");
                sb.Append("</td>");
                sb.Append("</tr>");
                sb.Append("</table>");
            }

            if ((plexEpisodes.Any() || embyEp.Any()) && !settings.DisableTv)
            {
                sb.Append("<br /><br /><h1 style=\"text-align: center;\">New TV</h1><br /><br />");
                sb.Append(
                "<table class=\"tv-table\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100 %; \">");
                sb.Append("<tr>");
                sb.Append("<td style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 14px; vertical-align: top; \">");
                sb.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100 %; \">");
                sb.Append("<tr>");
                await ProcessPlexTv(plexEpisodes, sb);
                await ProcessEmbyTv(embyEp, sb);
                sb.Append("</tr>");
                sb.Append("</table>");
                sb.Append("</td>");
                sb.Append("</tr>");
                sb.Append("</table>");
            }

            return sb.ToString();
        }

        private async Task ProcessPlexMovies(IQueryable<PlexServerContent> plexContentToSend, StringBuilder sb)
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
                var info = await _movieApi.GetMovieInformationWithExtraInfo(movieDbId);
                var mediaurl = content.Url;
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
                    _log.LogError(e, "Error when Processing Plex Movies {0}", info.Title);
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

        private async Task ProcessEmbyMovies(IQueryable<EmbyContent> embyContent, StringBuilder sb)
        {
            int count = 0;
            var ordered = embyContent.OrderByDescending(x => x.AddedAt);
            foreach (var content in ordered)
            {
                var theMovieDbId = content.TheMovieDbId;
                if (!content.TheMovieDbId.HasValue())
                {
                    var imdbId = content.ImdbId;
                    var findResult = await _movieApi.Find(imdbId, ExternalSource.imdb_id);
                    var result = findResult.movie_results?.FirstOrDefault();
                    if (result == null)
                    {
                        continue;
                    }

                    theMovieDbId = result.id.ToString();
                }

                var mediaurl = content.Url;
                var info = await _movieApi.GetMovieInformationWithExtraInfo(StringHelper.IntParseLinq(theMovieDbId));
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
                    _log.LogError(e, "Error when processing Emby Movies {0}", info.Title);
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
            catch (Exception)
            {
                // Swallow, couldn't parse the date
            }

            AddTitle(sb, $"https://www.imdb.com/title/{info.ImdbId}/", $"{info.Title} {releaseDate}");
            AddParagraph(sb, info.Overview);

            if (info.Genres.Any())
            {
                AddGenres(sb,
                    $"Genres: {string.Join(", ", info.Genres.Select(x => x.Name.ToString()).ToArray())}");
            }
        }

        private async Task ProcessPlexTv(HashSet<PlexEpisode> plexContent, StringBuilder sb)
        {
            var series = new List<PlexServerContent>();
            foreach (var plexEpisode in plexContent)
            {
                var alreadyAdded = series.FirstOrDefault(x => x.Key == plexEpisode.Series.Key);
                if (alreadyAdded != null)
                {
                    var episodeExists = alreadyAdded.Episodes.Any(x => x.Key == plexEpisode.Key);
                    if (!episodeExists)
                    {
                        alreadyAdded.Episodes.Add(plexEpisode);
                    }
                }
                else
                {
                    plexEpisode.Series.Episodes = new List<PlexEpisode> { plexEpisode };
                    series.Add(plexEpisode.Series);
                }
            }

            int count = 0;
            var orderedTv = series.OrderByDescending(x => x.AddedAt);
            foreach (var t in orderedTv)
            {
                try
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
                    var banner = info.image?.original;
                    if (!string.IsNullOrEmpty(banner))
                    {
                        banner = banner.Replace("http", "https"); // Always use the Https banners
                    }
                    
                    var tvInfo = await _movieApi.GetTVInfo(t.TheMovieDbId);
                    if (tvInfo != null && tvInfo.backdrop_path.HasValue())
                    {

                        AddBackgroundInsideTable(sb, $"https://image.tmdb.org/t/p/w500{tvInfo.backdrop_path}"); 
                    }
                    else
                    {
                        AddBackgroundInsideTable(sb, $"https://image.tmdb.org/t/p/w1280/");
                    }
                    AddPosterInsideTable(sb, banner);
                    AddMediaServerUrl(sb, t.Url, banner);
                    AddInfoTable(sb);

                    var title = $"{t.Title} ({t.ReleaseYear})";
                    AddTitle(sb, $"https://www.imdb.com/title/{info.externals.imdb}/", title);

                    // Group by the season number
                    var results = t.Episodes.GroupBy(p => p.SeasonNumber,
                        (key, g) => new
                        {
                            SeasonNumber = key,
                            Episodes = g.ToList()
                        }
                    );

                    // Group the episodes
                    var finalsb = new StringBuilder();
                    foreach (var epInformation in results.OrderBy(x => x.SeasonNumber))
                    {
                        var orderedEpisodes = epInformation.Episodes.OrderBy(x => x.EpisodeNumber).ToList();
                        var epSb = new StringBuilder();
                        for (var i = 0; i < orderedEpisodes.Count; i++)
                        {
                            var ep = orderedEpisodes[i];
                            if (i < orderedEpisodes.Count - 1)
                            {
                                epSb.Append($"{ep.EpisodeNumber},");
                            }
                            else
                            {
                                epSb.Append($"{ep.EpisodeNumber}");
                            }

                        }
                        finalsb.Append($"Season: {epInformation.SeasonNumber} - Episodes: {epSb}");
                        finalsb.Append("<br />");
                    }

                    var summary = info.summary;
                    if (summary.Length > 280)
                    {
                        summary = summary.Remove(280);
                        summary = summary + "...</p>";
                    }
                    AddTvParagraph(sb, finalsb.ToString(), summary);

                    if (info.genres.Any())
                    {
                        AddGenres(sb, $"Genres: {string.Join(", ", info.genres.Select(x => x.ToString()).ToArray())}");
                    }
                    count += 1;

                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error when processing Plex TV {0}", t.Title);
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

        private async Task ProcessEmbyTv(HashSet<EmbyEpisode> embyContent, StringBuilder sb)
        {
            var series = new List<EmbyContent>();
            foreach (var episode in embyContent)
            {
                var alreadyAdded = series.FirstOrDefault(x => x.EmbyId == episode.Series.EmbyId);
                if (alreadyAdded != null)
                {
                    alreadyAdded.Episodes.Add(episode);
                }
                else
                {
                    episode.Series.Episodes = new List<EmbyEpisode>
                    {
                        episode
                    };
                    series.Add(episode.Series);
                }
            }

            int count = 0;
            var orderedTv = series.OrderByDescending(x => x.AddedAt);
            foreach (var t in orderedTv)
            {
                try
                {
                    if (!t.TvDbId.HasValue())
                    {
                        continue;
                    }

                    int.TryParse(t.TvDbId, out var tvdbId);
                    var info = await _tvApi.ShowLookupByTheTvDbId(tvdbId);
                    if (info == null)
                    {
                        continue;
                    }

                    var banner = info.image?.original;
                    if (!string.IsNullOrEmpty(banner))
                    {
                        banner = banner.Replace("http", "https"); // Always use the Https banners
                    }

                    var tvInfo = await _movieApi.GetTVInfo(t.TheMovieDbId);
                    if (tvInfo != null && tvInfo.backdrop_path.HasValue())
                    {

                        AddBackgroundInsideTable(sb, $"https://image.tmdb.org/t/p/w500{tvInfo.backdrop_path}");
                    }
                    else
                    {
                        AddBackgroundInsideTable(sb, $"https://image.tmdb.org/t/p/w1280/");
                    }
                    AddPosterInsideTable(sb, banner);
                    AddMediaServerUrl(sb, t.Url, banner);
                    AddInfoTable(sb);
                    AddTitle(sb, $"https://www.imdb.com/title/{info.externals.imdb}/", $"{t.Title} ({info.premiered.Remove(4)})");

                    // Group by the season number
                    var results = t.Episodes?.GroupBy(p => p.SeasonNumber,
                        (key, g) => new
                        {
                            SeasonNumber = key,
                            Episodes = g.ToList()
                        }
                    );

                    // Group the episodes
                    var finalsb = new StringBuilder();
                    foreach (var epInformation in results.OrderBy(x => x.SeasonNumber))
                    {
                        var orderedEpisodes = epInformation.Episodes.OrderBy(x => x.EpisodeNumber).ToList();
                        var epSb = new StringBuilder();
                        for (var i = 0; i < orderedEpisodes.Count; i++)
                        {
                            var ep = orderedEpisodes[i];
                            if (i < orderedEpisodes.Count - 1)
                            {
                                epSb.Append($"{ep.EpisodeNumber},");
                            }
                            else
                            {
                                epSb.Append($"{ep.EpisodeNumber}");
                            }

                        }
                        finalsb.Append($"Season: {epInformation.SeasonNumber} - Episodes: {epSb}");
                        finalsb.Append("<br />");
                    }

                    var summary = info.summary;
                    if (summary.Length > 280)
                    {
                        summary = summary.Remove(280);
                        summary = summary + "...</p>";
                    }
                    AddTvParagraph(sb, finalsb.ToString(), summary);

                    if (info.genres.Any())
                    {
                        AddGenres(sb, $"Genres: {string.Join(", ", info.genres.Select(x => x.ToString()).ToArray())}");
                    }
                    count += 1;

                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error when processing Emby TV {0}", t.Title);
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
                _plex?.Dispose();
                _emby?.Dispose();
                _newsletterSettings?.Dispose();
                _customizationSettings?.Dispose();
                _emailSettings.Dispose();
                _recentlyAddedLog.Dispose();
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