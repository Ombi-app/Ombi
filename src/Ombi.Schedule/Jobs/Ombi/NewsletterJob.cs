using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            ISettingsService<EmailNotificationSettings> emailSettings, INotificationTemplatesRepository templateRepo)
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

        public async Task Start()
        {
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

            // Get the Content
            var plexContent = _plex.GetAll().Include(x => x.Episodes);
            var embyContent = _emby.GetAll().Include(x => x.Episodes);

            var addedLog = _recentlyAddedLog.GetAll();
            var addedPlexLogIds = addedLog.Where(x => x.Type == RecentlyAddedType.Plex).Select(x => x.ContentId);
            var addedEmbyLogIds = addedLog.Where(x => x.Type == RecentlyAddedType.Emby).Select(x => x.ContentId);

            // Filter out the ones that we haven't sent yet
            var plexContentToSend = plexContent.Where(x => !addedPlexLogIds.Contains(x.Id));
            var embyContentToSend = embyContent.Where(x => !addedEmbyLogIds.Contains(x.Id));

            var body = await BuildHtml(plexContentToSend, embyContentToSend);

            var email = new NewsletterTemplate();

            var customization = await _customizationSettings.GetSettingsAsync();

            var html = email.LoadTemplate(template.Subject, template.Message, body, customization.Logo);

            await _email.Send(new NotificationMessage {Message = html, Subject = template.Subject, To = "tidusjar@gmail.com"}, emailSettings);
        }

        private async Task<string> BuildHtml(IQueryable<PlexServerContent> plexContentToSend, IQueryable<EmbyContent> embyContentToSend)
        {
            var sb = new StringBuilder();

            sb.Append("<h1>New Movies:</h1><br /><br />");
            await ProcessPlexMovies(plexContentToSend.Where(x => x.Type == PlexMediaTypeEntity.Movie), sb);
            await ProcessEmbyMovies(embyContentToSend.Where(x => x.Type == EmbyMediaType.Movie), sb);

            sb.Append("<h1>New Episodes:</h1><br /><br />");
            await ProcessPlexTv(plexContentToSend.Where(x => x.Type == PlexMediaTypeEntity.Show), sb);
            await ProcessEmbyMovies(embyContentToSend.Where(x => x.Type == EmbyMediaType.Series), sb);

            return sb.ToString();
        }

        private async Task ProcessPlexMovies(IQueryable<PlexServerContent> plexContentToSend, StringBuilder sb)
        {
            sb.Append(
                "<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            var ordered = plexContentToSend.OrderByDescending(x => x.AddedAt);
            foreach (var content in ordered)
            {
                if (content.TheMovieDbId.IsNullOrEmpty())
                {
                    // Maybe we should try the ImdbId?
                    if (content.ImdbId.HasValue())
                    {
                        var findResult = await _movieApi.Find(content.ImdbId, ExternalSource.imdb_id);

                        var movieId = findResult.movie_results?[0]?.id ?? 0;
                        content.TheMovieDbId = movieId.ToString();
                    }
                }

                int.TryParse(content.TheMovieDbId, out var movieDbId);
                var info = await _movieApi.GetMovieInformationWithExtraInfo(movieDbId);
                if (info == null)
                {
                    continue;
                }
                try
                {
                    AddImageInsideTable(sb, $"https://image.tmdb.org/t/p/original{info.BackdropPath}");

                    sb.Append("<tr>");
                    TableData(sb);

                    Href(sb, $"https://www.imdb.com/title/{info.ImdbId}/");
                    Header(sb, 3, $"{info.Title} {info.ReleaseDate ?? string.Empty}");
                    EndTag(sb, "a");

                    if (info.Genres.Any())
                    {
                        AddParagraph(sb,
                            $"Genre: {string.Join(", ", info.Genres.Select(x => x.Name.ToString()).ToArray())}");
                    }

                    AddParagraph(sb, info.Overview);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    EndLoopHtml(sb);
                }
            }
        }
        private async Task ProcessEmbyMovies(IQueryable<EmbyContent> embyContent, StringBuilder sb)
        {
            sb.Append(
                "<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            var ordered = embyContent.OrderByDescending(x => x.AddedAt);
            foreach (var content in ordered)
            {
                int.TryParse(content.ProviderId, out var movieDbId);
                var info = await _movieApi.GetMovieInformationWithExtraInfo(movieDbId);
                if (info == null)
                {
                    continue;
                }
                try
                {
                    AddImageInsideTable(sb, $"https://image.tmdb.org/t/p/original{info.BackdropPath}");

                    sb.Append("<tr>");
                    TableData(sb);

                    Href(sb, $"https://www.imdb.com/title/{info.ImdbId}/");
                    Header(sb, 3, $"{info.Title} {info.ReleaseDate ?? string.Empty}");
                    EndTag(sb, "a");

                    if (info.Genres.Any())
                    {
                        AddParagraph(sb,
                            $"Genre: {string.Join(", ", info.Genres.Select(x => x.Name.ToString()).ToArray())}");
                    }

                    AddParagraph(sb, info.Overview);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    EndLoopHtml(sb);
                }
            }
        }

        private async Task ProcessPlexTv(IQueryable<PlexServerContent> plexContent, StringBuilder sb)
        {
            var orderedTv = plexContent.OrderByDescending(x => x.AddedAt);
            sb.Append(
                "<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
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
                    AddImageInsideTable(sb, banner);

                    sb.Append("<tr>");
                    sb.Append(
                        "<td align=\"center\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");

                    var title = $"{t.Title} {t.ReleaseYear}";

                    Href(sb, $"https://www.imdb.com/title/{info.externals.imdb}/");
                    Header(sb, 3, title);
                    EndTag(sb, "a");

                    // Group by the season number
                    var results = t.Episodes?.GroupBy(p => p.SeasonNumber,
                        (key, g) => new
                        {
                            SeasonNumber = key,
                            Episodes = g.ToList()
                        }
                    );

                    // Group the episodes
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
                        AddParagraph(sb, $"Season: {epInformation.SeasonNumber}, Episode: {epSb}");
                    }

                    if (info.genres.Any())
                    {
                        AddParagraph(sb, $"Genre: {string.Join(", ", info.genres.Select(x => x.ToString()).ToArray())}");
                    }

                    AddParagraph(sb, info.summary);
                }
                catch (Exception e)
                {
                    //Log.Error(e);
                }
                finally
                {
                    EndLoopHtml(sb);
                }
            }
            sb.Append("</table><br /><br />");

        }

        private async Task ProcessEmbyTv(IQueryable<EmbyContent> plexContent, StringBuilder sb)
        {
            var orderedTv = plexContent.OrderByDescending(x => x.AddedAt);
            sb.Append(
                "<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            foreach (var t in orderedTv)
            {
                try
                {
                    int.TryParse(t.ProviderId, out var tvdbId);
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
                    AddImageInsideTable(sb, banner);

                    sb.Append("<tr>");
                    sb.Append(
                        "<td align=\"center\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");
                    
                    Href(sb, $"https://www.imdb.com/title/{info.externals.imdb}/");
                    Header(sb, 3, t.Title);
                    EndTag(sb, "a");

                    // Group by the season number
                    var results = t.Episodes?.GroupBy(p => p.SeasonNumber,
                        (key, g) => new
                        {
                            SeasonNumber = key,
                            Episodes = g.ToList()
                        }
                    );

                    // Group the episodes
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
                        AddParagraph(sb, $"Season: {epInformation.SeasonNumber}, Episode: {epSb}");
                    }

                    if (info.genres.Any())
                    {
                        AddParagraph(sb, $"Genre: {string.Join(", ", info.genres.Select(x => x.ToString()).ToArray())}");
                    }

                    AddParagraph(sb, info.summary);
                }
                catch (Exception e)
                {
                    //Log.Error(e);
                }
                finally
                {
                    EndLoopHtml(sb);
                }
            }
            sb.Append("</table><br /><br />");
        }

        private void EndLoopHtml(StringBuilder sb)
        {
            //NOTE: BR have to be in TD's as per html spec or it will be put outside of the table...
            //Source: http://stackoverflow.com/questions/6588638/phantom-br-tag-rendered-by-browsers-prior-to-table-tag
            sb.Append("<hr />");
            sb.Append("<br />");
            sb.Append("<br />");
            sb.Append("</td>");
            sb.Append("</tr>");
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