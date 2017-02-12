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
using NLog;
using Ombi.Api;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Emby;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Services.Jobs.Templates;
using Ombi.Store.Models;
using Ombi.Store.Models.Emby;
using Ombi.Store.Repository;
using EmbyMediaType = Ombi.Store.Models.Plex.EmbyMediaType;

namespace Ombi.Services.Jobs.RecentlyAddedNewsletter
{
    public class EmbyAddedNewsletter : HtmlTemplateGenerator, IEmbyAddedNewsletter
    {
        public EmbyAddedNewsletter(IEmbyApi api, ISettingsService<EmbySettings> embySettings,
            ISettingsService<EmailNotificationSettings> email,
               ISettingsService<NewletterSettings> newsletter, IRepository<RecentlyAddedLog> log,
            IRepository<EmbyContent> embyContent, IRepository<EmbyEpisodes> episodes)
        {
            Api = api;
            EmbySettings = embySettings;
            EmailSettings = email;
            NewsletterSettings = newsletter;
            Content = embyContent;
            MovieApi = new TheMovieDbApi();
            TvApi = new TvMazeApi();
            Episodes = episodes;
            RecentlyAddedLog = log;
        }

        private IEmbyApi Api { get; }
        private TheMovieDbApi MovieApi { get; }
        private TvMazeApi TvApi { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
        private ISettingsService<EmailNotificationSettings> EmailSettings { get; }
        private ISettingsService<NewletterSettings> NewsletterSettings { get; }
        private IRepository<EmbyContent> Content { get; }
        private IRepository<EmbyEpisodes> Episodes { get; }
        private IRepository<RecentlyAddedLog> RecentlyAddedLog { get; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string GetNewsletterHtml(bool test)
        {
            try
            {
               return GetHtml(test);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return string.Empty;
            }
        }

        private class EmbyRecentlyAddedModel
        {
            public EmbyInformation EmbyInformation { get; set; }
            public EmbyContent EmbyContent { get; set; }
            public List<EmbyEpisodeInformation> EpisodeInformation { get; set; }
        }

        private string GetHtml(bool test)
        {
            var sb = new StringBuilder();
            var embySettings = EmbySettings.GetSettings();

            var embyContent = Content.GetAll().ToList();

            var series = embyContent.Where(x => x.Type == EmbyMediaType.Series).ToList();
            var episodes = Episodes.GetAll().ToList();
            var movie = embyContent.Where(x => x.Type == EmbyMediaType.Movie).ToList();

            var recentlyAdded = RecentlyAddedLog.GetAll().ToList();

            var firstRun = !recentlyAdded.Any();

            var filteredMovies = movie.Where(m => recentlyAdded.All(x => x.ProviderId != m.ProviderId)).ToList();
            var filteredEp = episodes.Where(m => recentlyAdded.All(x => x.ProviderId != m.ProviderId)).ToList();


            var info = new List<EmbyRecentlyAddedModel>();
            foreach (var m in filteredMovies)
            {

                var i = Api.GetInformation(m.EmbyId, Ombi.Api.Models.Emby.EmbyMediaType.Movie,
                    embySettings.ApiKey, embySettings.AdministratorId, embySettings.FullUri);
                info.Add(new EmbyRecentlyAddedModel
                {
                    EmbyInformation = i,
                    EmbyContent = m
                });
            }
            GenerateMovieHtml(info, sb);

            info.Clear();
            foreach (var t in series)
            {
                var i = Api.GetInformation(t.EmbyId, Ombi.Api.Models.Emby.EmbyMediaType.Series,
                    embySettings.ApiKey, embySettings.AdministratorId, embySettings.FullUri);
                var ep = filteredEp.Where(x => x.ParentId == t.EmbyId);

                if (ep.Any())
                {
                    var episodeList = new List<EmbyEpisodeInformation>();
                    foreach (var embyEpisodese in ep)
                    {
                        var epInfo = Api.GetInformation(embyEpisodese.EmbyId, Ombi.Api.Models.Emby.EmbyMediaType.Episode,
                            embySettings.ApiKey, embySettings.AdministratorId, embySettings.FullUri);
                        episodeList.Add(epInfo.EpisodeInformation);
                    }
                    info.Add(new EmbyRecentlyAddedModel
                    {
                        EmbyContent = t,
                        EmbyInformation = i,
                        EpisodeInformation = episodeList
                    });
                }
            }
            GenerateTvHtml(info, sb);

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
                        ProviderId = a.ProviderId,
                        AddedAt = DateTime.UtcNow
                    });
                }
            }

            var escapedHtml = new string(html.Where(c => !char.IsControl(c)).ToArray());
            Log.Debug(escapedHtml);
            return escapedHtml;
        }

        private void GenerateMovieHtml(IEnumerable<EmbyRecentlyAddedModel> recentlyAddedMovies, StringBuilder sb)
        {
            var movies = recentlyAddedMovies?.ToList() ?? new List<EmbyRecentlyAddedModel>();
            if (!movies.Any())
            {
                return;
            }
            var orderedMovies = movies.OrderByDescending(x => x.EmbyContent.AddedAt).Select(x => x.EmbyInformation.MovieInformation).ToList();
            sb.Append("<h1>New Movies:</h1><br /><br />");
            sb.Append(
                "<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            foreach (var movie in orderedMovies)
            {
                try
                {

                    var imdbId = movie.ProviderIds.Imdb;
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
                catch (Exception e)
                {
                    Log.Error(e);
                    Log.Error("Error for movie with IMDB Id = {0}", movie.ProviderIds.Imdb);
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
        private void GenerateTvHtml(List<EmbyRecentlyAddedModel> tv, StringBuilder sb)
        {
            if (!tv.Any())
            {
                return;
            }
            var orderedTv = tv.OrderByDescending(x => x.EmbyContent.AddedAt).ToList();

            // TV
            sb.Append("<h1>New Episodes:</h1><br /><br />");
            sb.Append(
                "<table border=\"0\" cellpadding=\"0\"  align=\"center\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\">");
            foreach (var t in orderedTv)
            {
                var seriesItem = t.EmbyInformation.SeriesInformation;
                var relatedEpisodes = t.EpisodeInformation;
                
                
                try
                {
                    var info = TvApi.ShowLookupByTheTvDbId(int.Parse(seriesItem.ProviderIds.Tvdb));

                    var banner = info.image?.original;
                    if (!string.IsNullOrEmpty(banner))
                    {
                        banner = banner.Replace("http", "https"); // Always use the Https banners
                    }
                    AddImageInsideTable(sb, banner);

                    sb.Append("<tr>");
                    sb.Append(
                        "<td align=\"center\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");

                    var title = $"{seriesItem.Name} {seriesItem.PremiereDate.Year}";

                    Href(sb, $"https://www.imdb.com/title/{info.externals.imdb}/");
                    Header(sb, 3, title);
                    EndTag(sb, "a");

                    var results = relatedEpisodes.GroupBy(p => p.ParentIndexNumber,
                        (key, g) => new
                        {
                            ParentIndexNumber = key,
                            IndexNumber = g.ToList()
                        }
                    );
                    // Group the episodes
                    foreach (var embyEpisodeInformation in results.OrderBy(x => x.ParentIndexNumber))
                    {
                        var epSb = new StringBuilder();
                        for (var i = 0; i < embyEpisodeInformation.IndexNumber.Count; i++)
                        {
                            var ep = embyEpisodeInformation.IndexNumber[i];
                            if (i < embyEpisodeInformation.IndexNumber.Count)
                            {
                                epSb.Append($"{ep.IndexNumber},");
                            }
                            else
                            {
                                epSb.Append(ep);
                            }
                        }
                        AddParagraph(sb, $"Season: {embyEpisodeInformation.ParentIndexNumber}, Episode: {epSb}");
                    }

                    if (info.genres.Any())
                    {
                        AddParagraph(sb, $"Genre: {string.Join(", ", info.genres.Select(x => x.ToString()).ToArray())}");
                    }

                    AddParagraph(sb, string.IsNullOrEmpty(seriesItem.Overview) ? info.summary : seriesItem.Overview);
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