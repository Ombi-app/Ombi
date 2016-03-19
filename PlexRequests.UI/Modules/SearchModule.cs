#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SearchModule.cs
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
using System.Globalization;
using System.Linq;

using Nancy;
using Nancy.Responses.Negotiation;

using NLog;

using PlexRequests.Api;
using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Notification;
using PlexRequests.Store;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class SearchModule : BaseModule
    {
        public SearchModule(ICacheProvider cache, ISettingsService<CouchPotatoSettings> cpSettings,
            ISettingsService<PlexRequestSettings> prSettings, IAvailabilityChecker checker,
            IRequestService request, ISonarrApi sonarrApi, ISettingsService<SonarrSettings> sonarrSettings,
            ICouchPotatoApi cpApi) : base("search")
        {
            CpService = cpSettings;
            PrService = prSettings;
            MovieApi = new TheMovieDbApi();
            TvApi = new TheTvDbApi();
            Cache = cache;
            Checker = checker;
            RequestService = request;
            SonarrApi = sonarrApi;
            SonarrService = sonarrSettings;
            CouchPotatoApi = cpApi;

            Get["/"] = parameters => RequestLoad();

            Get["movie/{searchTerm}"] = parameters => SearchMovie((string)parameters.searchTerm);
            Get["tv/{searchTerm}"] = parameters => SearchTvShow((string)parameters.searchTerm);

            Get["movie/upcoming"] = parameters => UpcomingMovies();
            Get["movie/playing"] = parameters => CurrentlyPlayingMovies();

            Post["request/movie"] = parameters => RequestMovie((int)Request.Form.movieId);
            Post["request/tv"] = parameters => RequestTvShow((int)Request.Form.tvId, (bool)Request.Form.latest);
        }
        private TheMovieDbApi MovieApi { get; }
        private ICouchPotatoApi CouchPotatoApi { get; }
        private ISonarrApi SonarrApi { get; }
        private TheTvDbApi TvApi { get; }
        private IRequestService RequestService { get; }
        private ICacheProvider Cache { get; }
        private ISettingsService<CouchPotatoSettings> CpService { get; }
        private ISettingsService<PlexRequestSettings> PrService { get; }
        private ISettingsService<SonarrSettings> SonarrService { get; }
        private IAvailabilityChecker Checker { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private string AuthToken => Cache.GetOrSet(CacheKeys.TvDbToken, TvApi.Authenticate, 50);

        private Negotiator RequestLoad()
        {
            var settings = PrService.GetSettings();

            Log.Trace("Loading Index");
            return View["Search/Index", settings];
        }

        private Response SearchMovie(string searchTerm)
        {
            Log.Trace("Searching for Movie {0}", searchTerm);
            var movies = MovieApi.SearchMovie(searchTerm);
            var result = movies.Result;
            return Response.AsJson(result);
        }

        private Response SearchTvShow(string searchTerm)
        {
            Log.Trace("Searching for TV Show {0}", searchTerm);
            //var tvShow = TvApi.SearchTv(searchTerm, AuthToken);
            var tvShow = new TvMazeApi().Search(searchTerm);
            if (!tvShow.Any())
            {
                Log.Trace("TV Show data is null");
                return Response.AsJson("");
            }
            var model = new List<SearchTvShowViewModel>();

            foreach (var t in tvShow)
            {
                model.Add(new SearchTvShowViewModel
                {
                    // We are constructing the banner with the id: 
                    // http://thetvdb.com/banners/_cache/posters/ID-1.jpg
                    Banner = t.show.image?.medium,
                    FirstAired = t.show.premiered,
                    Id = t.show.id,
                    ImdbId = t.show.externals?.imdb,
                    Network = t.show.network?.name,
                    NetworkId = t.show.network?.id.ToString(),
                    Overview = t.show.summary.RemoveHtml(),
                    Rating = t.score.ToString(CultureInfo.CurrentUICulture),
                    Runtime = t.show.runtime.ToString(),
                    SeriesId = t.show.id,
                    SeriesName = t.show.name,
                    
                    Status = t.show.status,
                });
            }

            Log.Trace("Returning TV Show results: ");
            Log.Trace(model.DumpJson());
            return Response.AsJson(model);
        }

        private Response UpcomingMovies() // TODO : Not used
        {
            var movies = MovieApi.GetUpcomingMovies();
            var result = movies.Result;
            Log.Trace("Movie Upcoming Results: ");
            Log.Trace(result.DumpJson());
            return Response.AsJson(result);
        }

        private Response CurrentlyPlayingMovies() // TODO : Not used
        {
            var movies = MovieApi.GetCurrentPlayingMovies();
            var result = movies.Result;
            Log.Trace("Movie Currently Playing Results: ");
            Log.Trace(result.DumpJson());
            return Response.AsJson(result);
        }

        private Response RequestMovie(int movieId)
        {
            Log.Info("Requesting movie with id {0}", movieId);
            if (RequestService.CheckRequest(movieId))
            {
                Log.Trace("movie with id {0} exists", movieId);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Movie has already been requested!" });
            }

            Log.Debug("movie with id {0} doesnt exists", movieId);
            var cpSettings = CpService.GetSettings();
            if (cpSettings.ApiKey == null)
            {
                Log.Warn("CP apiKey is null");
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "CouchPotato is not yet configured, If you are the Admin, please log in." });
            }

            Log.Trace("Settings: ");
            Log.Trace(cpSettings.DumpJson);

            var movieApi = new TheMovieDbApi();
            var movieInfo = movieApi.GetMovieInformation(movieId).Result;
            Log.Trace("Getting movie info from TheMovieDb");
            Log.Trace(movieInfo.DumpJson);

//#if !DEBUG
            if (CheckIfTitleExistsInPlex(movieInfo.Title, movieInfo.ReleaseDate?.Year.ToString()))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = $"{movieInfo.Title} is already in Plex!" });
            }
//#endif

            var model = new RequestedModel
            {
                ProviderId = movieInfo.Id,
                Type = RequestType.Movie,
                Overview = movieInfo.Overview,
                ImdbId = movieInfo.ImdbId,
                PosterPath = "http://image.tmdb.org/t/p/w150/" + movieInfo.PosterPath,
                Title = movieInfo.Title,
                ReleaseDate = movieInfo.ReleaseDate ?? DateTime.MinValue,
                Status = movieInfo.Status,
                RequestedDate = DateTime.Now,
                Approved = false,
                RequestedBy = Session[SessionKeys.UsernameKey].ToString(),
                Issues = IssueState.None,
            };


            var settings = PrService.GetSettings();
            Log.Trace(settings.DumpJson());
            if (!settings.RequireApproval)
            {
                Log.Info("Adding movie to CP (No approval required)");
                var result = CouchPotatoApi.AddMovie(model.ImdbId, cpSettings.ApiKey, model.Title, cpSettings.FullUri,cpSettings.ProfileId);
                Log.Debug("Adding movie to CP result {0}", result);
                if (result)
                {
                    model.Approved = true;
                    Log.Debug("Adding movie to database requests (No approval required)");
                    RequestService.AddRequest(model);

                    return Response.AsJson(new JsonResponseModel { Result = true });
                }
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Something went wrong adding the movie to CouchPotato! Please check your settings." });
            }

            try
            {
                Log.Debug("Adding movie to database requests");
                var id = RequestService.AddRequest(model);

                NotificationService.Publish(model.Title, model.RequestedBy);

                return Response.AsJson(new JsonResponseModel { Result = true });
            }
            catch (Exception e)
            {
                Log.Fatal(e);

                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Something went wrong adding the movie to CouchPotato! Please check your settings." });
            }
        }

        /// <summary>
        /// Requests the tv show.
        /// </summary>
        /// <param name="showId">The show identifier.</param>
        /// <param name="latest">if set to <c>true</c> [latest].</param>
        /// <returns></returns>
        private Response RequestTvShow(int showId, bool latest)
        {
            if (RequestService.CheckRequest(showId))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "TV Show has already been requested!" });
            }

            var tvApi = new TvMazeApi();

            var showInfo = tvApi.ShowLookup(showId);

//#if !DEBUG
            if (CheckIfTitleExistsInPlex(showInfo.name, showInfo.premiered.Substring(0,4))) // Take only the year Format = 2014-01-01
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = $"{showInfo.name} is already in Plex!" });
            }
//#endif

            DateTime firstAir;
            DateTime.TryParse(showInfo.premiered, out firstAir);

            var model = new RequestedModel
            {
                ProviderId = showInfo.id,
                Type = RequestType.TvShow,
                Overview = showInfo.summary.RemoveHtml(),
                PosterPath = showInfo.image?.medium,
                Title = showInfo.name,
                ReleaseDate = firstAir,
                Status = showInfo.status,
                RequestedDate = DateTime.Now,
                Approved = false,
                RequestedBy = Session[SessionKeys.UsernameKey].ToString(),
                Issues = IssueState.None,
                LatestTv = latest
            };


            var settings = PrService.GetSettings();
            if (!settings.RequireApproval)
            {
                var sonarrSettings = SonarrService.GetSettings();
                int qualityProfile;
                int.TryParse(sonarrSettings.QualityProfile, out qualityProfile);
                var result = SonarrApi.AddSeries(model.ProviderId, model.Title, qualityProfile,
                    sonarrSettings.SeasonFolders, sonarrSettings.RootPath, model.LatestTv, sonarrSettings.ApiKey,
                    sonarrSettings.FullUri);
                if (result != null)
                {
                    model.Approved = true;
                    Log.Debug("Adding tv to database requests (No approval required)");
                    RequestService.AddRequest(model);

                    return Response.AsJson(new JsonResponseModel { Result = true });
                }
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Something went wrong adding the movie to CouchPotato! Please check your settings." });
            }

            RequestService.AddRequest(model);
            NotificationService.Publish(model.Title, model.RequestedBy);

            return Response.AsJson(new { Result = true });
        }
        private string GetTvDbAuthToken(TheTvDbApi api)
        {
            return Cache.GetOrSet(CacheKeys.TvDbToken, api.Authenticate, 50);
        }

        private bool CheckIfTitleExistsInPlex(string title, string year)
        {
            var result = Checker.IsAvailable(title, year);
            return result;
        }
    }
}