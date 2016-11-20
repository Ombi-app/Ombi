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
using PlexRequests.Api.Models.Music;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Notification;
using PlexRequests.Store;
using PlexRequests.UI.Helpers;
using PlexRequests.UI.Models;
using System.Threading.Tasks;

using Nancy.Extensions;

using Newtonsoft.Json;

using PlexRequests.Api.Models.Sonarr;
using PlexRequests.Api.Models.Tv;
using PlexRequests.Core.Models;
using PlexRequests.Core.Queue;
using PlexRequests.Helpers.Analytics;
using PlexRequests.Helpers.Permissions;
using PlexRequests.Store.Models;
using PlexRequests.Store.Models.Plex;
using PlexRequests.Store.Repository;

using TMDbLib.Objects.General;

using Action = PlexRequests.Helpers.Analytics.Action;
using EpisodesModel = PlexRequests.Store.EpisodesModel;
using ISecurityExtensions = PlexRequests.Core.ISecurityExtensions;

namespace PlexRequests.UI.Modules
{
    public class SearchModule : BaseAuthModule
    {
        public SearchModule(ICacheProvider cache, ISettingsService<CouchPotatoSettings> cpSettings,
            ISettingsService<PlexRequestSettings> prSettings, IAvailabilityChecker checker,
            IRequestService request, ISonarrApi sonarrApi, ISettingsService<SonarrSettings> sonarrSettings,
            ISettingsService<SickRageSettings> sickRageService, ICouchPotatoApi cpApi, ISickRageApi srApi,
            INotificationService notify, IMusicBrainzApi mbApi, IHeadphonesApi hpApi,
            ISettingsService<HeadphonesSettings> hpService,
            ICouchPotatoCacher cpCacher, ISonarrCacher sonarrCacher, ISickRageCacher sickRageCacher, IPlexApi plexApi,
            ISettingsService<PlexSettings> plexService, ISettingsService<AuthenticationSettings> auth,
            IRepository<UsersToNotify> u, ISettingsService<EmailNotificationSettings> email,
            IIssueService issue, IAnalytics a, IRepository<RequestLimit> rl, ITransientFaultQueue tfQueue, IRepository<PlexContent> content, ISecurityExtensions security)
            : base("search", prSettings, security)
        {
            Auth = auth;
            PlexService = plexService;
            PlexApi = plexApi;
            CpService = cpSettings;
            PrService = prSettings;
            MovieApi = new TheMovieDbApi();
            Cache = cache;
            Checker = checker;
            CpCacher = cpCacher;
            SonarrCacher = sonarrCacher;
            SickRageCacher = sickRageCacher;
            RequestService = request;
            SonarrApi = sonarrApi;
            SonarrService = sonarrSettings;
            CouchPotatoApi = cpApi;
            SickRageService = sickRageService;
            SickrageApi = srApi;
            NotificationService = notify;
            MusicBrainzApi = mbApi;
            HeadphonesApi = hpApi;
            HeadphonesService = hpService;
            UsersToNotifyRepo = u;
            EmailNotificationSettings = email;
            IssueService = issue;
            Analytics = a;
            RequestLimitRepo = rl;
            FaultQueue = tfQueue;
            TvApi = new TvMazeApi();
            PlexContentRepository = content;


            Get["SearchIndex", "/", true] = async (x, ct) => await RequestLoad();

            Get["movie/{searchTerm}", true] = async (x, ct) => await SearchMovie((string)x.searchTerm);
            Get["tv/{searchTerm}", true] = async (x, ct) => await SearchTvShow((string)x.searchTerm);
            Get["music/{searchTerm}", true] = async (x, ct) => await SearchAlbum((string)x.searchTerm);
            Get["music/coverArt/{id}"] = p => GetMusicBrainzCoverArt((string)p.id);

            Get["movie/upcoming", true] = async (x, ct) => await UpcomingMovies();
            Get["movie/playing", true] = async (x, ct) => await CurrentlyPlayingMovies();

            Post["request/movie", true] = async (x, ct) => await RequestMovie((int)Request.Form.movieId);
            Post["request/tv", true] =
                async (x, ct) => await RequestTvShow((int)Request.Form.tvId, (string)Request.Form.seasons);
            Post["request/tvEpisodes", true] = async (x, ct) => await RequestTvShow(0, "episode");
            Post["request/album", true] = async (x, ct) => await RequestAlbum((string)Request.Form.albumId);
            
            Get["/seasons"] = x => GetSeasons();
            Get["/episodes", true] = async (x, ct) => await GetEpisodes();
        }

        private IRepository<PlexContent> PlexContentRepository { get; }
        private TvMazeApi TvApi { get; }
        private IPlexApi PlexApi { get; }
        private TheMovieDbApi MovieApi { get; }
        private INotificationService NotificationService { get; }
        private ICouchPotatoApi CouchPotatoApi { get; }
        private ISonarrApi SonarrApi { get; }
        private ISickRageApi SickrageApi { get; }
        private IRequestService RequestService { get; }
        private ICacheProvider Cache { get; }
        private ISettingsService<AuthenticationSettings> Auth { get; }
        private ISettingsService<PlexSettings> PlexService { get; }
        private ISettingsService<CouchPotatoSettings> CpService { get; }
        private ISettingsService<PlexRequestSettings> PrService { get; }
        private ISettingsService<SonarrSettings> SonarrService { get; }
        private ISettingsService<SickRageSettings> SickRageService { get; }
        private ISettingsService<HeadphonesSettings> HeadphonesService { get; }
        private ISettingsService<EmailNotificationSettings> EmailNotificationSettings { get; }
        private IAvailabilityChecker Checker { get; }
        private ICouchPotatoCacher CpCacher { get; }
        private ISonarrCacher SonarrCacher { get; }
        private ISickRageCacher SickRageCacher { get; }
        private IMusicBrainzApi MusicBrainzApi { get; }
        private IHeadphonesApi HeadphonesApi { get; }
        private IRepository<UsersToNotify> UsersToNotifyRepo { get; }
        private IIssueService IssueService { get; }
        private IAnalytics Analytics { get; }
        private ITransientFaultQueue FaultQueue { get; }
        private IRepository<RequestLimit> RequestLimitRepo { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        private async Task<Negotiator> RequestLoad()
        {

            var settings = await PrService.GetSettingsAsync();

            return View["Search/Index", settings];
        }

        private async Task<Response> UpcomingMovies()
        {
            Analytics.TrackEventAsync(Category.Search, Action.Movie, "Upcoming", Username,
                CookieHelper.GetAnalyticClientId(Cookies));
            return await ProcessMovies(MovieSearchType.Upcoming, string.Empty);
        }

        private async Task<Response> CurrentlyPlayingMovies()
        {
            Analytics.TrackEventAsync(Category.Search, Action.Movie, "CurrentlyPlaying", Username,
                CookieHelper.GetAnalyticClientId(Cookies));
            return await ProcessMovies(MovieSearchType.CurrentlyPlaying, string.Empty);
        }

        private async Task<Response> SearchMovie(string searchTerm)
        {
            Analytics.TrackEventAsync(Category.Search, Action.Movie, searchTerm, Username,
                CookieHelper.GetAnalyticClientId(Cookies));
            return await ProcessMovies(MovieSearchType.Search, searchTerm);
        }

        private async Task<Response> ProcessMovies(MovieSearchType searchType, string searchTerm)
        {
            List<MovieResult> apiMovies;

            switch (searchType)
            {
                case MovieSearchType.Search:
                    var movies = await MovieApi.SearchMovie(searchTerm).ConfigureAwait(false);
                    apiMovies = movies.Select(x =>
                            new MovieResult
                            {
                                Adult = x.Adult,
                                BackdropPath = x.BackdropPath,
                                GenreIds = x.GenreIds,
                                Id = x.Id,
                                OriginalLanguage = x.OriginalLanguage,
                                OriginalTitle = x.OriginalTitle,
                                Overview = x.Overview,
                                Popularity = x.Popularity,
                                PosterPath = x.PosterPath,
                                ReleaseDate = x.ReleaseDate,
                                Title = x.Title,
                                Video = x.Video,
                                VoteAverage = x.VoteAverage,
                                VoteCount = x.VoteCount
                            })
                        .ToList();
                    break;
                case MovieSearchType.CurrentlyPlaying:
                    apiMovies = await MovieApi.GetCurrentPlayingMovies();
                    break;
                case MovieSearchType.Upcoming:
                    apiMovies = await MovieApi.GetUpcomingMovies();
                    break;
                default:
                    apiMovies = new List<MovieResult>();
                    break;
            }

            var allResults = await RequestService.GetAllAsync();
            allResults = allResults.Where(x => x.Type == RequestType.Movie);

            var distinctResults = allResults.DistinctBy(x => x.ProviderId);
            var dbMovies = distinctResults.ToDictionary(x => x.ProviderId);


            var cpCached = CpCacher.QueuedIds();
            var content = PlexContentRepository.GetAll();
            var plexMovies = Checker.GetPlexMovies(content);
            var settings = await PrService.GetSettingsAsync();
            var viewMovies = new List<SearchMovieViewModel>();
            var counter = 0;
            foreach (var movie in apiMovies)
            {
                var imdbId = string.Empty;
                if (counter <= 5) // Let's only do it for the first 5 items
                {
                    var movieInfoTask = await MovieApi.GetMovieInformation(movie.Id).ConfigureAwait(false);
                    // TODO needs to be careful about this, it's adding extra time to search...
                    // https://www.themoviedb.org/talk/5807f4cdc3a36812160041f2
                    imdbId = movieInfoTask.ImdbId;
                    counter++;
                }

                var viewMovie = new SearchMovieViewModel
                {
                    Adult = movie.Adult,
                    BackdropPath = movie.BackdropPath,
                    GenreIds = movie.GenreIds,
                    Id = movie.Id,
                    OriginalLanguage = movie.OriginalLanguage,
                    OriginalTitle = movie.OriginalTitle,
                    Overview = movie.Overview,
                    Popularity = movie.Popularity,
                    PosterPath = movie.PosterPath,
                    ReleaseDate = movie.ReleaseDate,
                    Title = movie.Title,
                    Video = movie.Video,
                    VoteAverage = movie.VoteAverage,
                    VoteCount = movie.VoteCount
                };
                var canSee = CanUserSeeThisRequest(viewMovie.Id, settings.UsersCanViewOnlyOwnRequests, dbMovies);
                var plexMovie = Checker.GetMovie(plexMovies.ToArray(), movie.Title, movie.ReleaseDate?.Year.ToString(),
                    imdbId);
                if (plexMovie != null)
                {
                    viewMovie.Available = true;
                    viewMovie.PlexUrl = plexMovie.Url;
                }
                else if (dbMovies.ContainsKey(movie.Id) && canSee) // compare to the requests db
                {
                    var dbm = dbMovies[movie.Id];

                    viewMovie.Requested = true;
                    viewMovie.Approved = dbm.Approved;
                    viewMovie.Available = dbm.Available;
                }
                else if (cpCached.Contains(movie.Id) && canSee) // compare to the couchpotato db
                {
                    viewMovie.Requested = true;
                }

                viewMovies.Add(viewMovie);
            }

            return Response.AsJson(viewMovies);
        }

        private bool CanUserSeeThisRequest(int movieId, bool usersCanViewOnlyOwnRequests,
            Dictionary<int, RequestedModel> moviesInDb)
        {
            if (usersCanViewOnlyOwnRequests)
            {
                var result = moviesInDb.FirstOrDefault(x => x.Value.ProviderId == movieId);
                return result.Value == null || result.Value.UserHasRequested(Username);
            }

            return true;
        }

        private async Task<Response> SearchTvShow(string searchTerm)
        {

            Analytics.TrackEventAsync(Category.Search, Action.TvShow, searchTerm, Username,
                CookieHelper.GetAnalyticClientId(Cookies));
            var plexSettings = await PlexService.GetSettingsAsync();
            var prSettings = await PrService.GetSettingsAsync();
            var providerId = string.Empty;

            var apiTv = new List<TvMazeSearch>();
            await Task.Factory.StartNew(() => new TvMazeApi().Search(searchTerm)).ContinueWith((t) =>
            {
                apiTv = t.Result;
            });

            var allResults = await RequestService.GetAllAsync();
            allResults = allResults.Where(x => x.Type == RequestType.TvShow);
            var distinctResults = allResults.DistinctBy(x => x.ProviderId);
            var dbTv = distinctResults.ToDictionary(x => x.ProviderId);

            if (!apiTv.Any())
            {
                return Response.AsJson("");
            }

            var sonarrCached = SonarrCacher.QueuedIds();
            var sickRageCache = SickRageCacher.QueuedIds(); // consider just merging sonarr/sickrage arrays
            var content = PlexContentRepository.GetAll();
            var plexTvShows = Checker.GetPlexTvShows(content);

            var viewTv = new List<SearchTvShowViewModel>();
            foreach (var t in apiTv)
            {
                var banner = t.show.image?.medium;
                if (!string.IsNullOrEmpty(banner))
                {
                    banner = banner.Replace("http", "https"); // Always use the Https banners
                }

                var viewT = new SearchTvShowViewModel
                {
                    Banner = banner,
                    FirstAired = t.show.premiered,
                    Id = t.show.externals?.thetvdb ?? 0,
                    ImdbId = t.show.externals?.imdb,
                    Network = t.show.network?.name,
                    NetworkId = t.show.network?.id.ToString(),
                    Overview = t.show.summary.RemoveHtml(),
                    Rating = t.score.ToString(CultureInfo.CurrentUICulture),
                    Runtime = t.show.runtime.ToString(),
                    SeriesId = t.show.id,
                    SeriesName = t.show.name,
                    Status = t.show.status,
                    DisableTvRequestsByEpisode = prSettings.DisableTvRequestsByEpisode,
                    DisableTvRequestsBySeason = prSettings.DisableTvRequestsBySeason
                };


                if (plexSettings.AdvancedSearch)
                {
                    providerId = viewT.Id.ToString();
                }

                var plexShow = Checker.GetTvShow(plexTvShows.ToArray(), t.show.name, t.show.premiered?.Substring(0, 4),
                    providerId);
                if (plexShow != null)
                {
                    viewT.Available = true;
                    viewT.PlexUrl = plexShow.Url;
                }
                else if (t.show?.externals?.thetvdb != null)
                {
                    var tvdbid = (int)t.show.externals.thetvdb;
                    if (dbTv.ContainsKey(tvdbid))
                    {
                        var dbt = dbTv[tvdbid];

                        viewT.Requested = true;
                        viewT.Episodes = dbt.Episodes.ToList();
                        viewT.Approved = dbt.Approved;
                    }
                    if (sonarrCached.Select(x => x.TvdbId).Contains(tvdbid) || sickRageCache.Contains(tvdbid))
                    // compare to the sonarr/sickrage db
                    {
                        viewT.Requested = true;
                    }
                }

                viewTv.Add(viewT);
            }

            return Response.AsJson(viewTv);
        }

        private async Task<Response> SearchAlbum(string searchTerm)
        {
            Analytics.TrackEventAsync(Category.Search, Action.Album, searchTerm, Username,
                CookieHelper.GetAnalyticClientId(Cookies));
            var apiAlbums = new List<Release>();
            await Task.Run(() => MusicBrainzApi.SearchAlbum(searchTerm)).ContinueWith((t) =>
            {
                apiAlbums = t.Result.releases ?? new List<Release>();
            });

            var allResults = await RequestService.GetAllAsync();
            allResults = allResults.Where(x => x.Type == RequestType.Album);

            var dbAlbum = allResults.ToDictionary(x => x.MusicBrainzId);

            var content = PlexContentRepository.GetAll();
            var plexAlbums = Checker.GetPlexAlbums(content);

            var viewAlbum = new List<SearchMusicViewModel>();
            foreach (var a in apiAlbums)
            {
                var viewA = new SearchMusicViewModel
                {
                    Title = a.title,
                    Id = a.id,
                    Artist = a.ArtistCredit?.Select(x => x.artist?.name).FirstOrDefault(),
                    Overview = a.disambiguation,
                    ReleaseDate = a.date,
                    TrackCount = a.TrackCount,
                    ReleaseType = a.status,
                    Country = a.country
                };

                DateTime release;
                DateTimeHelper.CustomParse(a.ReleaseEvents?.FirstOrDefault()?.date, out release);
                var artist = a.ArtistCredit?.FirstOrDefault()?.artist;
                var plexAlbum = Checker.GetAlbum(plexAlbums.ToArray(), a.title, release.ToString("yyyy"), artist?.name);
                if (plexAlbum != null)
                {
                    viewA.Available = true;
                    viewA.PlexUrl = plexAlbum.Url;
                }
                if (!string.IsNullOrEmpty(a.id) && dbAlbum.ContainsKey(a.id))
                {
                    var dba = dbAlbum[a.id];

                    viewA.Requested = true;
                    viewA.Approved = dba.Approved;
                    viewA.Available = dba.Available;
                }

                viewAlbum.Add(viewA);
            }
            return Response.AsJson(viewAlbum);
        }

        private async Task<Response> RequestMovie(int movieId)
        {
            if (Security.HasPermissions(User, Permissions.ReadOnlyUser) || !Security.HasPermissions(User, Permissions.RequestMovie))
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = "Sorry, you do not have the correct permissions to request a movie!"
                    });
            }
            var settings = await PrService.GetSettingsAsync();
            if (!await CheckRequestLimit(settings, RequestType.Movie))
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = "You have reached your weekly request limit for Movies! Please contact your admin."
                    });
            }

            Analytics.TrackEventAsync(Category.Search, Action.Request, "Movie", Username,
                CookieHelper.GetAnalyticClientId(Cookies));
            var movieInfo = await MovieApi.GetMovieInformation(movieId);
            var fullMovieName =
                $"{movieInfo.Title}{(movieInfo.ReleaseDate.HasValue ? $" ({movieInfo.ReleaseDate.Value.Year})" : string.Empty)}";

            var existingRequest = await RequestService.CheckRequestAsync(movieId);
            if (existingRequest != null)
            {
                // check if the current user is already marked as a requester for this movie, if not, add them
                if (!existingRequest.UserHasRequested(Username))
                {
                    existingRequest.RequestedUsers.Add(Username);
                    await RequestService.UpdateRequestAsync(existingRequest);
                }

                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = true,
                        Message =
                            settings.UsersCanViewOnlyOwnRequests
                                ? $"{fullMovieName} {Resources.UI.Search_SuccessfullyAdded}"
                                : $"{fullMovieName} {Resources.UI.Search_AlreadyRequested}"
                    });
            }

            try
            {

                var content = PlexContentRepository.GetAll();
                var movies = Checker.GetPlexMovies(content);
                if (Checker.IsMovieAvailable(movies.ToArray(), movieInfo.Title, movieInfo.ReleaseDate?.Year.ToString()))
                {
                    return
                        Response.AsJson(new JsonResponseModel
                        {
                            Result = false,
                            Message = $"{fullMovieName} is already in Plex!"
                        });
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = string.Format(Resources.UI.Search_CouldNotCheckPlex, fullMovieName)
                    });
            }
            //#endif

            var model = new RequestedModel
            {
                ProviderId = movieInfo.Id,
                Type = RequestType.Movie,
                Overview = movieInfo.Overview,
                ImdbId = movieInfo.ImdbId,
                PosterPath = movieInfo.PosterPath,
                Title = movieInfo.Title,
                ReleaseDate = movieInfo.ReleaseDate ?? DateTime.MinValue,
                Status = movieInfo.Status,
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                RequestedUsers = new List<string> { Username },
                Issues = IssueState.None,

            };
            try
            {
                if (ShouldAutoApprove(RequestType.Movie, settings, Username))
                {
                    var cpSettings = await CpService.GetSettingsAsync();
                    model.Approved = true;
                    if (cpSettings.Enabled)
                    {
                        Log.Info("Adding movie to CP (No approval required)");
                        var result = CouchPotatoApi.AddMovie(model.ImdbId, cpSettings.ApiKey, model.Title,
                            cpSettings.FullUri, cpSettings.ProfileId);
                        Log.Debug("Adding movie to CP result {0}", result);
                        if (result)
                        {
                            return
                                await
                                    AddRequest(model, settings,
                                        $"{fullMovieName} {Resources.UI.Search_SuccessfullyAdded}");
                        }

                        return Response.AsJson(new JsonResponseModel
                        {
                            Result = false,
                            Message = Resources.UI.Search_CouchPotatoError
                        });
                    }
                    model.Approved = true;
                    return await AddRequest(model, settings, $"{fullMovieName} {Resources.UI.Search_SuccessfullyAdded}");
                }


                return await AddRequest(model, settings, $"{fullMovieName} {Resources.UI.Search_SuccessfullyAdded}");
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                await FaultQueue.QueueItemAsync(model, movieInfo.Id.ToString(), RequestType.Movie, FaultType.RequestFault);

                await NotificationService.Publish(new NotificationModel
                {
                    DateTime = DateTime.Now,
                    User = Username,
                    RequestType = RequestType.Movie,
                    Title = model.Title,
                    NotificationType = NotificationType.ItemAddedToFaultQueue
                });

                return Response.AsJson(new JsonResponseModel
                {
                    Result = true,
                    Message = $"{fullMovieName} {Resources.UI.Search_SuccessfullyAdded}"
                });
            }
        }

        /// <summary>
        /// Requests the tv show.
        /// </summary>
        /// <param name="showId">The show identifier.</param>
        /// <param name="seasons">The seasons.</param>
        /// <returns></returns>
        private async Task<Response> RequestTvShow(int showId, string seasons)
        {
            if (Security.HasPermissions(User, Permissions.ReadOnlyUser) || !Security.HasPermissions(User, Permissions.RequestTvShow))
            {
                return
                    Response.AsJson(new JsonResponseModel()
                    {
                        Result = false,
                        Message = "Sorry, you do not have the correct permissions to request a TV Show!"
                    });
            }
            // Get the JSON from the request
            var req = (Dictionary<string, object>.ValueCollection)Request.Form.Values;
            EpisodeRequestModel episodeModel = null;
            if (req.Count == 1)
            {
                var json = req.FirstOrDefault()?.ToString();
                episodeModel = JsonConvert.DeserializeObject<EpisodeRequestModel>(json); // Convert it into the object
            }
            var episodeRequest = false;

            var settings = await PrService.GetSettingsAsync();
            if (!await CheckRequestLimit(settings, RequestType.TvShow))
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = Resources.UI.Search_WeeklyRequestLimitTVShow
                    });
            }
            Analytics.TrackEventAsync(Category.Search, Action.Request, "TvShow", Username,
                CookieHelper.GetAnalyticClientId(Cookies));

            var sonarrSettings = SonarrService.GetSettingsAsync();

            // This means we are requesting an episode rather than a whole series or season
            if (episodeModel != null)
            {
                episodeRequest = true;
                showId = episodeModel.ShowId;
                var s = await sonarrSettings;
                if (!s.Enabled)
                {
                    return
                        Response.AsJson(new JsonResponseModel
                        {
                            Message =
                                "This is currently only supported with Sonarr, Please enable Sonarr for this feature",
                            Result = false
                        });
                }
            }

            var showInfo = TvApi.ShowLookupByTheTvDbId(showId);
            DateTime firstAir;
            DateTime.TryParse(showInfo.premiered, out firstAir);
            string fullShowName = $"{showInfo.name} ({firstAir.Year})";

            var model = new RequestedModel
            {
                Type = RequestType.TvShow,
                Overview = showInfo.summary.RemoveHtml(),
                PosterPath = showInfo.image?.medium,
                Title = showInfo.name,
                ReleaseDate = firstAir,
                Status = showInfo.status,
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                RequestedUsers = new List<string> { Username },
                Issues = IssueState.None,
                ImdbId = showInfo.externals?.imdb ?? string.Empty,
                SeasonCount = showInfo.Season.Count,
                TvDbId = showId.ToString()
            };

            var seasonsList = new List<int>();
            switch (seasons)
            {
                case "first":
                    seasonsList.Add(1);
                    model.SeasonsRequested = "First";
                    break;
                case "latest":
                    seasonsList.Add(model.SeasonCount);
                    model.SeasonsRequested = "Latest";
                    break;
                case "all":
                    model.SeasonsRequested = "All";
                    break;
                case "episode":
                    model.Episodes = new List<EpisodesModel>();

                    foreach (var ep in episodeModel?.Episodes ?? new Models.EpisodesModel[0])
                    {
                        model.Episodes.Add(new EpisodesModel
                        {
                            EpisodeNumber = ep.EpisodeNumber,
                            SeasonNumber = ep.SeasonNumber
                        });
                    }
                    Analytics.TrackEventAsync(Category.Requests, Action.TvShow, $"Episode request for {model.Title}",
                        Username, CookieHelper.GetAnalyticClientId(Cookies));
                    break;
                default:
                    model.SeasonsRequested = seasons;
                    var split = seasons.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var seasonsCount = new int[split.Length];
                    for (var i = 0; i < split.Length; i++)
                    {
                        int tryInt;
                        int.TryParse(split[i], out tryInt);
                        seasonsCount[i] = tryInt;
                    }
                    seasonsList.AddRange(seasonsCount);
                    break;
            }

            model.SeasonList = seasonsList.ToArray();

            // check if the show/episodes have already been requested
            var existingRequest = await RequestService.CheckRequestAsync(showId);
            var difference = new List<EpisodesModel>();
            if (existingRequest != null)
            {
                if (episodeRequest)
                {
                    // Make sure we are not somehow adding dupes
                    difference = GetListDifferences(existingRequest.Episodes, episodeModel.Episodes).ToList();
                    if (difference.Any())
                    {
                        // Convert the request into the correct shape
                        var newEpisodes = episodeModel.Episodes?.Select(x => new EpisodesModel
                        {
                            SeasonNumber = x.SeasonNumber,
                            EpisodeNumber = x.EpisodeNumber
                        });

                        // Add it to the existing requests
                        existingRequest.Episodes.AddRange(newEpisodes ?? Enumerable.Empty<EpisodesModel>());

                        // It's technically a new request now, so set the status to not approved.
                        existingRequest.Approved = false;

                        return await AddUserToRequest(existingRequest, settings, fullShowName, true);
                    }
                    else
                    {
                        // We no episodes to approve
                        return
                            Response.AsJson(new JsonResponseModel
                            {
                                Result = false,
                                Message = $"{fullShowName} {Resources.UI.Search_AlreadyInPlex}"
                            });
                    }
                }
                else if (model.SeasonList.Except(existingRequest.SeasonList).Any())
                {
                    // This is a season being requested that we do not yet have
                    // Let's just continue
                }
                else
                {
                    return await AddUserToRequest(existingRequest, settings, fullShowName);
                }
            }

            try
            {

                var content = PlexContentRepository.GetAll();
                var shows = Checker.GetPlexTvShows(content);
                var providerId = string.Empty;
                var plexSettings = await PlexService.GetSettingsAsync();
                if (plexSettings.AdvancedSearch)
                {
                    providerId = showId.ToString();
                }
                if (episodeRequest)
                {
                    var cachedEpisodesTask = await Checker.GetEpisodes();
                    var cachedEpisodes = cachedEpisodesTask.ToList();
                    foreach (var d in difference) // difference is from an existing request
                    {
                        if (
                            cachedEpisodes.Any(
                                x =>
                                    x.SeasonNumber == d.SeasonNumber && x.EpisodeNumber == d.EpisodeNumber &&
                                    x.ProviderId == providerId))
                        {
                            return
                                Response.AsJson(new JsonResponseModel
                                {
                                    Result = false,
                                    Message =
                                        $"{fullShowName}  {d.SeasonNumber} - {d.EpisodeNumber} {Resources.UI.Search_AlreadyInPlex}"
                                });
                        }
                    }

                    var diff = await GetEpisodeRequestDifference(showId, model);
                    model.Episodes = diff.ToList();
                }
                else
                {
                    if (plexSettings.EnableTvEpisodeSearching)
                    {
                        foreach (var s in showInfo.Season)
                        {
                            var result = Checker.IsEpisodeAvailable(showId.ToString(), s.SeasonNumber, s.EpisodeNumber);
                            if (result)
                            {
                                return
                                    Response.AsJson(new JsonResponseModel
                                    {
                                        Result = false,
                                        Message = $"{fullShowName} {Resources.UI.Search_AlreadyInPlex}"
                                    });
                            }
                        }
                    }
                    else if (Checker.IsTvShowAvailable(shows.ToArray(), showInfo.name, showInfo.premiered?.Substring(0, 4),
                        providerId, model.SeasonList))
                    {
                        return
                            Response.AsJson(new JsonResponseModel
                            {
                                Result = false,
                                Message = $"{fullShowName} {Resources.UI.Search_AlreadyInPlex}"
                            });
                    }
                }
            }
            catch (Exception)
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = string.Format(Resources.UI.Search_CouldNotCheckPlex, fullShowName)
                    });
            }

            if (showInfo.externals?.thetvdb == null)
            {
                await FaultQueue.QueueItemAsync(model, showInfo.id.ToString(), RequestType.TvShow, FaultType.MissingInformation);
                await NotificationService.Publish(new NotificationModel
                {
                    DateTime = DateTime.Now,
                    User = Username,
                    RequestType = RequestType.TvShow,
                    Title = model.Title,
                    NotificationType = NotificationType.ItemAddedToFaultQueue
                });
                return Response.AsJson(new JsonResponseModel
                {
                    Result = true,
                    Message = $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}"
                });
            }

            model.ProviderId = showInfo.externals?.thetvdb ?? 0;

            try
            {
                if (ShouldAutoApprove(RequestType.TvShow, settings, Username))
                {
                    model.Approved = true;
                    var s = await sonarrSettings;
                    var sender = new TvSenderOld(SonarrApi, SickrageApi); // TODO put back
                    if (s.Enabled)
                    {
                        var result = await sender.SendToSonarr(s, model);
                        if (!string.IsNullOrEmpty(result?.title))
                        {
                            if (existingRequest != null)
                            {
                                return await UpdateRequest(model, settings,
                                    $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}");
                            }
                            return
                                await
                                    AddRequest(model, settings,
                                        $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}");
                        }
                        Log.Debug("Error with sending to sonarr.");
                        return
                            Response.AsJson(ValidationHelper.SendSonarrError(result?.ErrorMessages ?? new List<string>()));
                    }

                    var srSettings = SickRageService.GetSettings();
                    if (srSettings.Enabled)
                    {
                        var result = sender.SendToSickRage(srSettings, model);
                        if (result?.result == "success")
                        {
                            return await AddRequest(model, settings,
                                        $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}");
                        }
                        return
                            Response.AsJson(new JsonResponseModel
                            {
                                Result = false,
                                Message = result?.message ?? Resources.UI.Search_SickrageError
                            });
                    }

                    if (!srSettings.Enabled && !s.Enabled)
                    {
                        return await AddRequest(model, settings, $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}");
                    }

                    return
                        Response.AsJson(new JsonResponseModel { Result = false, Message = Resources.UI.Search_TvNotSetUp });

                }
                return await AddRequest(model, settings, $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}");
            }
            catch (Exception e)
            {
                await FaultQueue.QueueItemAsync(model, showInfo.id.ToString(), RequestType.TvShow, FaultType.RequestFault);
                await NotificationService.Publish(new NotificationModel
                {
                    DateTime = DateTime.Now,
                    User = Username,
                    RequestType = RequestType.TvShow,
                    Title = model.Title,
                    NotificationType = NotificationType.ItemAddedToFaultQueue
                });
                Log.Error(e);
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = true,
                        Message = $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}"
                    });
            }
        }

        private async Task<Response> AddUserToRequest(RequestedModel existingRequest, PlexRequestSettings settings,
            string fullShowName, bool episodeReq = false)
        {
            // check if the current user is already marked as a requester for this show, if not, add them
            if (!existingRequest.UserHasRequested(Username))
            {
                existingRequest.RequestedUsers.Add(Username);
            }
            if (settings.UsersCanViewOnlyOwnRequests || episodeReq)
            {
                return
                    await
                        UpdateRequest(existingRequest, settings,
                            $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}");
            }

            return
                await UpdateRequest(existingRequest, settings, $"{fullShowName} {Resources.UI.Search_AlreadyRequested}");
        }

        private bool ShouldSendNotification(RequestType type, PlexRequestSettings prSettings)
        {
            var sendNotification = ShouldAutoApprove(type, prSettings, Username)
                ? !prSettings.IgnoreNotifyForAutoApprovedRequests
                : true;

            if (IsAdmin)
            {
                sendNotification = false; // Don't bother sending a notification if the user is an admin

            }
            return sendNotification;
        }


        private async Task<Response> RequestAlbum(string releaseId)
        {
            if (Security.HasPermissions(User, Permissions.ReadOnlyUser) || !Security.HasPermissions(User, Permissions.RequestMusic))
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = "Sorry, you do not have the correct permissions to request music!"
                    });
            }

            var settings = await PrService.GetSettingsAsync();
            if (!await CheckRequestLimit(settings, RequestType.Album))
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = Resources.UI.Search_WeeklyRequestLimitAlbums
                    });
            }
            Analytics.TrackEventAsync(Category.Search, Action.Request, "Album", Username,
                CookieHelper.GetAnalyticClientId(Cookies));
            var existingRequest = await RequestService.CheckRequestAsync(releaseId);

            if (existingRequest != null)
            {
                if (!existingRequest.UserHasRequested(Username))
                {
                    existingRequest.RequestedUsers.Add(Username);
                    await RequestService.UpdateRequestAsync(existingRequest);
                }
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = true,
                        Message =
                            settings.UsersCanViewOnlyOwnRequests
                                ? $"{existingRequest.Title} {Resources.UI.Search_SuccessfullyAdded}"
                                : $"{existingRequest.Title} {Resources.UI.Search_AlreadyRequested}"
                    });
            }

            var albumInfo = MusicBrainzApi.GetAlbum(releaseId);
            DateTime release;
            DateTimeHelper.CustomParse(albumInfo.ReleaseEvents?.FirstOrDefault()?.date, out release);

            var artist = albumInfo.ArtistCredits?.FirstOrDefault()?.artist;
            if (artist == null)
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = Resources.UI.Search_MusicBrainzError
                    });
            }


            var content = PlexContentRepository.GetAll();
            var albums = Checker.GetPlexAlbums(content);
            var alreadyInPlex = Checker.IsAlbumAvailable(albums.ToArray(), albumInfo.title, release.ToString("yyyy"),
                artist.name);

            if (alreadyInPlex)
            {
                return Response.AsJson(new JsonResponseModel
                {
                    Result = false,
                    Message = $"{albumInfo.title} {Resources.UI.Search_AlreadyInPlex}"
                });
            }

            var img = GetMusicBrainzCoverArt(albumInfo.id);

            var model = new RequestedModel
            {
                Title = albumInfo.title,
                MusicBrainzId = albumInfo.id,
                Overview = albumInfo.disambiguation,
                PosterPath = img,
                Type = RequestType.Album,
                ProviderId = 0,
                RequestedUsers = new List<string> { Username },
                Status = albumInfo.status,
                Issues = IssueState.None,
                RequestedDate = DateTime.UtcNow,
                ReleaseDate = release,
                ArtistName = artist.name,
                ArtistId = artist.id
            };

            try
            {
                if (ShouldAutoApprove(RequestType.Album, settings, Username))
                {
                    model.Approved = true;
                    var hpSettings = HeadphonesService.GetSettings();

                    if (!hpSettings.Enabled)
                    {
                        await RequestService.AddRequestAsync(model);
                        return
                            Response.AsJson(new JsonResponseModel
                            {
                                Result = true,
                                Message = $"{model.Title} {Resources.UI.Search_SuccessfullyAdded}"
                            });
                    }

                    var sender = new HeadphonesSender(HeadphonesApi, hpSettings, RequestService);
                    await sender.AddAlbum(model);
                    return await AddRequest(model, settings, $"{model.Title} {Resources.UI.Search_SuccessfullyAdded}");
                }

                return await AddRequest(model, settings, $"{model.Title} {Resources.UI.Search_SuccessfullyAdded}");
            }
            catch (Exception e)
            {
                Log.Error(e);
                await FaultQueue.QueueItemAsync(model, albumInfo.id, RequestType.Album, FaultType.RequestFault);

                await NotificationService.Publish(new NotificationModel
                {
                    DateTime = DateTime.Now,
                    User = Username,
                    RequestType = RequestType.Album,
                    Title = model.Title,
                    NotificationType = NotificationType.ItemAddedToFaultQueue
                });
                throw;
            }
        }

        private string GetMusicBrainzCoverArt(string id)
        {
            var coverArt = MusicBrainzApi.GetCoverArt(id);
            var firstImage = coverArt?.images?.FirstOrDefault();
            var img = string.Empty;

            if (firstImage != null)
            {
                img = firstImage.thumbnails?.small ?? firstImage.image;
            }

            return img;
        }
        
        private Response GetSeasons()
        {
            var seriesId = (int)Request.Query.tvId;
            var show = TvApi.ShowLookupByTheTvDbId(seriesId);
            var seasons = TvApi.GetSeasons(show.id);
            var model = seasons.Select(x => x.number);
            return Response.AsJson(model);
        }

        private async Task<Response> GetEpisodes()
        {
            var seriesId = (int)Request.Query.tvId;
            var model = await GetEpisodes(seriesId);

            return Response.AsJson(model);
        }

        private async Task<List<EpisodeListViewModel>> GetEpisodes(int providerId)
        {
            var s = await SonarrService.GetSettingsAsync();
            var sonarrEnabled = s.Enabled;
            var allResults = await RequestService.GetAllAsync();

            var seriesTask = Task.Run(
                () =>
                {
                    if (sonarrEnabled)
                    {
                        var allSeries = SonarrApi.GetSeries(s.ApiKey, s.FullUri);
                        var selectedSeries = allSeries.FirstOrDefault(x => x.tvdbId == providerId) ?? new Series();
                        return selectedSeries;
                    }
                    return new Series();
                });

            var model = new List<EpisodeListViewModel>();

            var requests = allResults as RequestedModel[] ?? allResults.ToArray();

            var existingRequest = requests.FirstOrDefault(x => x.Type == RequestType.TvShow && x.TvDbId == providerId.ToString());
            var show = await Task.Run(() => TvApi.ShowLookupByTheTvDbId(providerId));
            var tvMaxeEpisodes = await Task.Run(() => TvApi.EpisodeLookup(show.id));

            var sonarrEpisodes = new List<SonarrEpisodes>();
            if (sonarrEnabled)
            {
                var sonarrSeries = await seriesTask;
                var sonarrEp = SonarrApi.GetEpisodes(sonarrSeries.id.ToString(), s.ApiKey, s.FullUri);
                sonarrEpisodes = sonarrEp?.ToList() ?? new List<SonarrEpisodes>();
            }

            var plexCacheTask = await Checker.GetEpisodes(providerId);
            var plexCache = plexCacheTask.ToList();
            foreach (var ep in tvMaxeEpisodes)
            {
                var requested = existingRequest?.Episodes
                    .Any(episodesModel =>
                    ep.number == episodesModel.EpisodeNumber && ep.season == episodesModel.SeasonNumber) ?? false;

                var alreadyInPlex = plexCache.Any(x => x.EpisodeNumber == ep.number && x.SeasonNumber == ep.season);
                var inSonarr = sonarrEpisodes.Any(x => x.seasonNumber == ep.season && x.episodeNumber == ep.number && x.hasFile);

                model.Add(new EpisodeListViewModel
                {
                    Id = show.id,
                    SeasonNumber = ep.season,
                    EpisodeNumber = ep.number,
                    Requested = requested || alreadyInPlex || inSonarr,
                    Name = ep.name,
                    EpisodeId = ep.id
                });
            }
            return model;

        }

        public async Task<bool> CheckRequestLimit(PlexRequestSettings s, RequestType type)
        {
            if (IsAdmin)
                return true;

            if (s.ApprovalWhiteList.Contains(Username))
                return true;

            var requestLimit = GetRequestLimitForType(type, s);
            if (requestLimit == 0)
            {
                return true;
            }

            var limit = await RequestLimitRepo.GetAllAsync();
            var usersLimit = limit.FirstOrDefault(x => x.Username == Username && x.RequestType == type);
            if (usersLimit == null)
            {
                // Have not set a requestLimit yet
                return true;
            }

            return requestLimit > usersLimit.RequestCount;
        }

        private int GetRequestLimitForType(RequestType type, PlexRequestSettings s)
        {
            int requestLimit;
            switch (type)
            {
                case RequestType.Movie:
                    requestLimit = s.MovieWeeklyRequestLimit;
                    break;
                case RequestType.TvShow:
                    requestLimit = s.TvWeeklyRequestLimit;
                    break;
                case RequestType.Album:
                    requestLimit = s.AlbumWeeklyRequestLimit;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return requestLimit;
        }

        private async Task<Response> AddRequest(RequestedModel model, PlexRequestSettings settings, string message)
        {
            await RequestService.AddRequestAsync(model);

            if (ShouldSendNotification(model.Type, settings))
            {
                var notificationModel = new NotificationModel
                {
                    Title = model.Title,
                    User = Username,
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.NewRequest,
                    RequestType = model.Type
                };
                await NotificationService.Publish(notificationModel);
            }

            var limit = await RequestLimitRepo.GetAllAsync();
            var usersLimit = limit.FirstOrDefault(x => x.Username == Username && x.RequestType == model.Type);
            if (usersLimit == null)
            {
                await RequestLimitRepo.InsertAsync(new RequestLimit
                {
                    Username = Username,
                    RequestType = model.Type,
                    FirstRequestDate = DateTime.UtcNow,
                    RequestCount = 1
                });
            }
            else
            {
                usersLimit.RequestCount++;
                await RequestLimitRepo.UpdateAsync(usersLimit);
            }

            return Response.AsJson(new JsonResponseModel { Result = true, Message = message });
        }

        private async Task<Response> UpdateRequest(RequestedModel model, PlexRequestSettings settings, string message)
        {
            await RequestService.UpdateRequestAsync(model);

            if (ShouldSendNotification(model.Type, settings))
            {
                var notificationModel = new NotificationModel
                {
                    Title = model.Title,
                    User = Username,
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.NewRequest,
                    RequestType = model.Type
                };
                await NotificationService.Publish(notificationModel);
            }

            var limit = await RequestLimitRepo.GetAllAsync();
            var usersLimit = limit.FirstOrDefault(x => x.Username == Username && x.RequestType == model.Type);
            if (usersLimit == null)
            {
                await RequestLimitRepo.InsertAsync(new RequestLimit
                {
                    Username = Username,
                    RequestType = model.Type,
                    FirstRequestDate = DateTime.UtcNow,
                    RequestCount = 1
                });
            }
            else
            {
                usersLimit.RequestCount++;
                await RequestLimitRepo.UpdateAsync(usersLimit);
            }

            return Response.AsJson(new JsonResponseModel { Result = true, Message = message });
        }

        private IEnumerable<Store.EpisodesModel> GetListDifferences(IEnumerable<EpisodesModel> existing, IEnumerable<Models.EpisodesModel> request)
        {
            var newRequest = request
                .Select(r =>
                    new EpisodesModel
                    {
                        SeasonNumber = r.SeasonNumber,
                        EpisodeNumber = r.EpisodeNumber
                    }).ToList();

            return newRequest.Except(existing);
        }

        private async Task<IEnumerable<EpisodesModel>> GetEpisodeRequestDifference(int showId, RequestedModel model)
        {
            var episodes = await GetEpisodes(showId);
            var availableEpisodes = episodes.Where(x => x.Requested).ToList();
            var available = availableEpisodes.Select(a => new EpisodesModel { EpisodeNumber = a.EpisodeNumber, SeasonNumber = a.SeasonNumber }).ToList();

            var diff = model.Episodes.Except(available);
            return diff;
        }

        public bool ShouldAutoApprove(RequestType requestType, PlexRequestSettings prSettings, string username)
        {
            var admin = Security.HasPermissions(Context.CurrentUser, Permissions.Administrator);
            // if the user is an admin or they are whitelisted, they go ahead and allow auto-approval
            if (admin || prSettings.ApprovalWhiteList.Any(x => x.Equals(username, StringComparison.OrdinalIgnoreCase))) return true;

            // check by request type if the category requires approval or not
            switch (requestType)
            {
                case RequestType.Movie:
                    return Security.HasPermissions(User, Permissions.AutoApproveMovie) ||
                           !prSettings.RequireMovieApproval;
                case RequestType.TvShow:
                    return Security.HasPermissions(User, Permissions.AutoApproveTv) ||
                        !prSettings.RequireTvShowApproval;
                case RequestType.Album:
                    return Security.HasPermissions(User, Permissions.AutoApproveAlbum) ||
                        !prSettings.RequireMusicApproval;
                default:
                    return false;
            }
        }
    }
}
