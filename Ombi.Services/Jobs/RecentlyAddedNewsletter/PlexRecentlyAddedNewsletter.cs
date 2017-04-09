#region Copyright

// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RecentlyAddedModel.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using Ombi.Api;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Emby;
using Ombi.Api.Models.Plex;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Services.Jobs.Templates;
using Ombi.Store.Models;
using Ombi.Store.Models.Plex;
using Ombi.Store.Repository;
using TMDbLib.Objects.Exceptions;
using PlexMediaType = Ombi.Store.Models.Plex.PlexMediaType;

namespace Ombi.Services.Jobs.RecentlyAddedNewsletter
{
    public class
        PlexRecentlyAddedNewsletter : HtmlTemplateGenerator, IPlexNewsletter
    {
        public PlexRecentlyAddedNewsletter(IPlexApi api, ISettingsService<PlexSettings> plexSettings,
            ISettingsService<EmailNotificationSettings> email,
               ISettingsService<NewletterSettings> newsletter, IRepository<RecentlyAddedLog> log,
            IRepository<PlexContent> embyContent, IRepository<PlexEpisodes> episodes)
        {
            Api = api;
            PlexSettings = plexSettings;
            EmailSettings = email;
            NewsletterSettings = newsletter;
            Content = embyContent;
            MovieApi = new TheMovieDbApi();
            TvApi = new TvMazeApi();
            Episodes = episodes;
            RecentlyAddedLog = log;
        }

        private IPlexApi Api { get; }
        private TheMovieDbApi MovieApi { get; }
        private TvMazeApi TvApi { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmailNotificationSettings> EmailSettings { get; }
        private ISettingsService<NewletterSettings> NewsletterSettings { get; }
        private IRepository<PlexContent> Content { get; }
        private IRepository<PlexEpisodes> Episodes { get; }
        private IRepository<RecentlyAddedLog> RecentlyAddedLog { get; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public Newsletter GetNewsletter(bool test)
        {
            try
            {
                return GetHtml(test);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        private class PlexRecentlyAddedModel
        {
            public PlexMetadata Metadata { get; set; }
            public PlexContent Content { get; set; }
            public List<PlexEpisodeMetadata> EpisodeMetadata { get; set; }
        }

        private Newsletter GetHtml(bool test)
        {
            var sb = new StringBuilder();
            var newsletter = new Newsletter();
            var plexSettings = PlexSettings.GetSettings();

            var plexContent = Content.GetAll().ToList();

            var series = plexContent.Where(x => x.Type == PlexMediaType.Show).ToList();
            var episodes = Episodes.GetAll().ToList();
            var movie = plexContent.Where(x => x.Type == PlexMediaType.Movie).ToList();

            var recentlyAdded = RecentlyAddedLog.GetAll().ToList();

            var firstRun = !recentlyAdded.Any();

            var filteredMovies = movie.Where(m => recentlyAdded.All(x => x.ProviderId != m.ProviderId)).ToList();
            var filteredEp = episodes.Where(m => recentlyAdded.All(x => x.ProviderId != m.RatingKey)).ToList();
            var filteredSeries = series.Where(x => recentlyAdded.All(c => c.ProviderId != x.ProviderId)).ToList();

            var info = new List<PlexRecentlyAddedModel>();

            if (test && !filteredMovies.Any())
            {
                // if this is a test make sure we show something
                filteredMovies = movie.Take(5).ToList();
            }
            foreach (var m in filteredMovies.OrderByDescending(x => x.AddedAt))
            {
                var i = Api.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri, m.ItemId);
                if (i.Video == null)
                {
                    continue;
                }
                info.Add(new PlexRecentlyAddedModel
                {
                    Metadata = i,
                    Content = m
                });
            }
            GenerateMovieHtml(info, sb);
            newsletter.MovieCount = info.Count;

            info.Clear();
            if (test && !filteredEp.Any() && episodes.Any())
            {
                // if this is a test make sure we show something
                filteredEp = episodes.Take(5).ToList();
            }
            if (filteredEp.Any())
            {
                var recentlyAddedModel = new List<PlexRecentlyAddedModel>();
                foreach (var plexEpisodes in filteredEp)
                {
                    // Find related series item
                    var relatedSeries = series.FirstOrDefault(x => x.ProviderId == plexEpisodes.ProviderId);

                    if (relatedSeries == null)
                    {
                        continue;
                    }

                    // Get series information
                    var i = Api.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri, relatedSeries.ItemId);

                    var episodeInfo = Api.GetEpisodeMetaData(plexSettings.PlexAuthToken, plexSettings.FullUri, plexEpisodes.RatingKey);
                    // Check if we already have this series
                    var existingSeries = recentlyAddedModel.FirstOrDefault(x =>
                        x.Metadata.Directory.RatingKey == i.Directory.RatingKey);

                    if (existingSeries != null)
                    {
                        existingSeries.EpisodeMetadata.Add(episodeInfo);
                    }
                    else
                    {
                        recentlyAddedModel.Add(new PlexRecentlyAddedModel
                        {
                            Metadata = i,
                            EpisodeMetadata = new List<PlexEpisodeMetadata>() { episodeInfo },
                            Content = relatedSeries
                        });
                    }
                }

                info.AddRange(recentlyAddedModel);
            }
            else
            {
                if (test && !filteredSeries.Any())
                {
                    // if this is a test make sure we show something
                    filteredSeries = series.Take(5).ToList();
                }
                foreach (var t in filteredSeries.OrderByDescending(x => x.AddedAt))
                {
                    var i = Api.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri, t.ItemId);
                    if (i.Directory == null)
                    {
                        continue;

                    }

                    info.Add(new PlexRecentlyAddedModel
                    {
                        Metadata = i,
                        Content = t
                    });
                }
            }
            GenerateTvHtml(info, sb);
            newsletter.TvCount = info.Count;

            var template = new RecentlyAddedTemplate();
            var html = template.LoadTemplate(sb.ToString());
            Log.Debug("Loaded the template");

            if (!test || firstRun)
            {
                foreach (var a in filteredMovies)
                {
                    RecentlyAddedLog.Insert(new RecentlyAddedLog
                    {
                        ProviderId = a.ProviderId,
                        AddedAt = DateTime.UtcNow
                    });
                }
                foreach (var a in filteredEp)
                {
                    RecentlyAddedLog.Insert(new RecentlyAddedLog
                    {
                        ProviderId = a.RatingKey,
                        AddedAt = DateTime.UtcNow
                    });
                }
                foreach (var a in filteredSeries)
                {
                    RecentlyAddedLog.Insert(new RecentlyAddedLog
                    {
                        ProviderId = a.ProviderId,
                        AddedAt = DateTime.UtcNow
                    });
                }
            }

            var escapedHtml = new string(html.Where(c => !char.IsControl(c)).ToArray());
            Log.Debug(escapedHtml);
            newsletter.Html = escapedHtml;
            return newsletter;
        }

        private void GenerateMovieHtml(IEnumerable<PlexRecentlyAddedModel> recentlyAddedMovies, StringBuilder sb)
        {
            var movies = recentlyAddedMovies?.ToList() ?? new List<PlexRecentlyAddedModel>();
            if (!movies.Any())
            {
                return;
            }
            var orderedMovies = movies.OrderByDescending(x => x.Content.AddedAt).ToList();
            sb.Append("<h1>New Movies:</h1><br /><br />");
            sb.Append(
                "<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            foreach (var movie in orderedMovies)
            {
                // We have a try within a try so we can catch the rate limit without ending the loop (finally block)
                try
                {
                    try
                    {

                        var imdbId = PlexHelper.GetProviderIdFromPlexGuid(movie.Metadata.Video.Guid);
                        var info = MovieApi.GetMovieInformation(imdbId).Result;
                        if (info == null)
                        {
                            throw new Exception($"Movie with Imdb id {imdbId} returned null from the MovieApi");
                        }
                        AddImageInsideTable(sb, $"https://image.tmdb.org/t/p/w500{info.BackdropPath}");

                        sb.Append("<tr>");
                        sb.Append(
                            "<td align=\"center\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");

                        Href(sb, $"https://www.imdb.com/title/{info.ImdbId}/");
                        Header(sb, 3, $"{info.Title} {info.ReleaseDate?.ToString("yyyy") ?? string.Empty}");
                        EndTag(sb, "a");

                        if (info.Genres.Any())
                        {
                            AddParagraph(sb,
                                $"Genre: {string.Join(", ", info.Genres.Select(x => x.Name.ToString()).ToArray())}");
                        }

                        AddParagraph(sb, info.Overview);
                    }
                    catch (RequestLimitExceededException limit)
                    {
                        // We have hit a limit, we need to now wait.
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                        Log.Info(limit);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    Log.Error("Error for movie with IMDB Id = {0}", movie.Metadata.Video.Guid);
                }
                finally
                {
                    EndLoopHtml(sb);
                }

            }
            sb.Append("</table><br /><br />");
        }

        private class TvModel
        {
            public EmbySeriesInformation Series { get; set; }
            public List<EmbyEpisodeInformation> Episodes { get; set; }
        }
        private void GenerateTvHtml(IEnumerable<PlexRecentlyAddedModel> recenetlyAddedTv, StringBuilder sb)
        {
            var tv = recenetlyAddedTv?.ToList() ?? new List<PlexRecentlyAddedModel>();

            if (!tv.Any())
            {
                return;
            }
            var orderedTv = tv.OrderByDescending(x => x.Content.AddedAt).ToList();

            // TV
            sb.Append("<h1>New Episodes:</h1><br /><br />");
            sb.Append(
                "<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            foreach (var t in orderedTv)
            {
                var relatedEpisodes = t.EpisodeMetadata ?? new List<PlexEpisodeMetadata>();

                try
                {
                    var info = TvApi.ShowLookupByTheTvDbId(int.Parse(PlexHelper.GetProviderIdFromPlexGuid(t?.Metadata?.Directory?.Guid ?? string.Empty)));

                    var banner = info.image?.original;
                    if (!string.IsNullOrEmpty(banner))
                    {
                        banner = banner.Replace("http", "https"); // Always use the Https banners
                    }
                    AddImageInsideTable(sb, banner);

                    sb.Append("<tr>");
                    sb.Append(
                        "<td align=\"center\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");

                    var title = $"{t.Content.Title} {t.Content.ReleaseYear}";

                    Href(sb, $"https://www.imdb.com/title/{info.externals.imdb}/");
                    Header(sb, 3, title);
                    EndTag(sb, "a");

                    // Group by the ParentIndex (season number)
                    var results = relatedEpisodes.GroupBy(p => p.Video.FirstOrDefault()?.ParentIndex,
                        (key, g) => new
                        {
                            ParentIndexNumber = key,
                            IndexNumber = g.ToList()
                        }
                    );
                    // Group the episodes
                    foreach (var epInformation in results.OrderBy(x => x.ParentIndexNumber))
                    {
                        var orderedEpisodes = epInformation.IndexNumber.OrderBy(x => Convert.ToInt32(x.Video.FirstOrDefault().Index)).ToList();
                        var epSb = new StringBuilder();
                        for (var i = 0; i < orderedEpisodes.Count; i++)
                        {
                            var ep = orderedEpisodes[i];
                            if (i < orderedEpisodes.Count - 1)
                            {
                                epSb.Append($"{ep.Video.FirstOrDefault().Index},");
                            }
                            else
                            {
                                epSb.Append($"{ep.Video.FirstOrDefault().Index}");
                            }

                        }
                        AddParagraph(sb, $"Season: {epInformation.ParentIndexNumber}, Episode: {epSb}");
                    }

                    if (info.genres.Any())
                    {
                        AddParagraph(sb, $"Genre: {string.Join(", ", info.genres.Select(x => x.ToString()).ToArray())}");
                    }

                    AddParagraph(sb, string.IsNullOrEmpty(t.Metadata.Directory.Summary) ? t.Metadata.Directory.Summary : info.summary);
                }
                catch (Exception e)
                {
                    Log.Error(e);
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

    }
}