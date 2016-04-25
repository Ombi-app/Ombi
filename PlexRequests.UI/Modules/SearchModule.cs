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
using Nancy.Security;

using NLog;

using PlexRequests.Api;
using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Music;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Helpers.Exceptions;
using PlexRequests.Services;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Notification;
using PlexRequests.Store;
using PlexRequests.UI.Helpers;
using PlexRequests.UI.Models;
using System.Threading.Tasks;
using PlexRequests.Api.Models.Tv;
using TMDbLib.Objects.General;

namespace PlexRequests.UI.Modules
{
    public class SearchModule : BaseAuthModule
    {
        public SearchModule(ICacheProvider cache, ISettingsService<CouchPotatoSettings> cpSettings,
            ISettingsService<PlexRequestSettings> prSettings, IAvailabilityChecker checker,
            IRequestService request, ISonarrApi sonarrApi, ISettingsService<SonarrSettings> sonarrSettings,
            ISettingsService<SickRageSettings> sickRageService, ICouchPotatoApi cpApi, ISickRageApi srApi,
            INotificationService notify, IMusicBrainzApi mbApi, IHeadphonesApi hpApi, ISettingsService<HeadphonesSettings> hpService, 
            ICouchPotatoCacher cpCacher, ISonarrCacher sonarrCacher, ISickRageCacher sickRageCacher, IPlexApi plexApi, 
            ISettingsService<PlexSettings> plexService, ISettingsService<AuthenticationSettings> auth) : base("search")
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


            Get["/"] = parameters => RequestLoad();

            Get["movie/{searchTerm}"] = parameters => SearchMovie((string)parameters.searchTerm);
            Get["tv/{searchTerm}"] = parameters => SearchTvShow((string)parameters.searchTerm);
            Get["music/{searchTerm}"] = parameters => SearchMusic((string)parameters.searchTerm);
            Get["music/coverArt/{id}"] = p => GetMusicBrainzCoverArt((string)p.id);

            Get["movie/upcoming"] = parameters => UpcomingMovies();
            Get["movie/playing"] = parameters => CurrentlyPlayingMovies();

            Post["request/movie"] = parameters => RequestMovie((int)Request.Form.movieId, (bool)Request.Form.notify);
            Post["request/tv"] = parameters => RequestTvShow((int)Request.Form.tvId, (string)Request.Form.seasons, (bool)Request.Form.notify);
            Post["request/album"] = parameters => RequestAlbum((string)Request.Form.albumId, (bool)Request.Form.notify);
        }
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
        private IAvailabilityChecker Checker { get; }
        private ICouchPotatoCacher CpCacher { get; }
        private ISonarrCacher SonarrCacher { get; }
        private ISickRageCacher SickRageCacher { get; }
        private IMusicBrainzApi MusicBrainzApi { get; }
        private IHeadphonesApi HeadphonesApi { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        private bool IsAdmin
        {
            get
            {
                return Context.CurrentUser.IsAuthenticated();
            }
        }

        private Negotiator RequestLoad()
        {
            var settings = PrService.GetSettings();

            Log.Trace("Loading Index");
            return View["Search/Index", settings];
        }

        private Response UpcomingMovies()
        {
            Log.Trace("Loading upcoming movies");
            return ProcessMovies(MovieSearchType.Upcoming, string.Empty);
        }

        private Response CurrentlyPlayingMovies()
        {
            Log.Trace("Loading currently playing movies");
            return ProcessMovies(MovieSearchType.CurrentlyPlaying, string.Empty);
        }

        private Response SearchMovie(string searchTerm)
        {
            Log.Trace("Searching for Movie {0}", searchTerm);
            return ProcessMovies(MovieSearchType.Search, searchTerm);
        }

        private Response ProcessMovies(MovieSearchType searchType, string searchTerm)
        {
            var taskList = new List<Task>();

            var apiMovies = new List<MovieResult>();
            taskList.Add(Task.Factory.StartNew(() =>
            {
                switch(searchType)
                {
                    case MovieSearchType.Search:
                        return MovieApi.SearchMovie(searchTerm).Result.Select(x => new MovieResult()
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
                        }).ToList();
                    case MovieSearchType.CurrentlyPlaying:
                        return MovieApi.GetCurrentPlayingMovies().Result.ToList();
                    case MovieSearchType.Upcoming:
                        return MovieApi.GetUpcomingMovies().Result.ToList();
                    default:
                        return new List<MovieResult>();
                }
            }).ContinueWith((t) =>
            {
                apiMovies = t.Result;
            }));

            Dictionary<int, RequestedModel> dbMovies = new Dictionary<int, RequestedModel>();
            taskList.Add(Task.Factory.StartNew(() =>
            {
                return RequestService.GetAll().Where(x => x.Type == RequestType.Movie);

            }).ContinueWith((t) =>
            {
                dbMovies = t.Result.ToDictionary(x => x.ProviderId);
            }));

            Task.WaitAll(taskList.ToArray());

            var cpCached = CpCacher.QueuedIds();
            var plexMovies = Checker.GetPlexMovies();
            var settings = PrService.GetSettings();
            var viewMovies = new List<SearchMovieViewModel>();
            foreach (MovieResult movie in apiMovies)
            {
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
                if (Checker.IsMovieAvailable(plexMovies.ToArray(), movie.Title, movie.ReleaseDate?.Year.ToString()))
                {
                    viewMovie.Available = true;
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

        private bool CanUserSeeThisRequest(int movieId, bool usersCanViewOnlyOwnRequests, Dictionary<int, RequestedModel> moviesInDb)
        {
            if (usersCanViewOnlyOwnRequests)
            {
                var result = moviesInDb.FirstOrDefault(x => x.Value.ProviderId == movieId);
                return result.Value != null && result.Value.UserHasRequested(Username);
            }

            return true;
        }

        private Response SearchTvShow(string searchTerm)
        {
            Log.Trace("Searching for TV Show {0}", searchTerm);

           var taskList = new List<Task>();

            var apiTv = new List<TvMazeSearch>();
            taskList.Add(Task.Factory.StartNew(() =>
            {
                return new TvMazeApi().Search(searchTerm);

            }).ContinueWith((t) =>
            {
                apiTv = t.Result;
            }));

            var dbTv = new Dictionary<int, RequestedModel>();
            taskList.Add(Task.Factory.StartNew(() =>
            {
                return RequestService.GetAll().Where(x => x.Type == RequestType.TvShow);

            }).ContinueWith((t) =>
            {
                dbTv = t.Result.ToDictionary(x => x.ProviderId);
            }));

            Task.WaitAll(taskList.ToArray());

            if (!apiTv.Any())
            {
                Log.Trace("TV Show data is null");
                return Response.AsJson("");
            }

            var sonarrCached = SonarrCacher.QueuedIds();
            var sickRageCache = SickRageCacher.QueuedIds(); // consider just merging sonarr/sickrage arrays
            var plexTvShows = Checker.GetPlexTvShows();

            var viewTv = new List<SearchTvShowViewModel>();
            foreach (var t in apiTv)
            {
                var viewT = new SearchTvShowViewModel
                {
                    // We are constructing the banner with the id: 
                    // http://thetvdb.com/banners/_cache/posters/ID-1.jpg
                    Banner = t.show.image?.medium,
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
                    Status = t.show.status
                };

                if (Checker.IsTvShowAvailable(plexTvShows.ToArray(), t.show.name, t.show.premiered?.Substring(0, 4)))
                {
                    viewT.Available = true;
                }
                else if (t.show?.externals?.thetvdb != null)
                {
                    int tvdbid = (int)t.show.externals.thetvdb;

                    if (dbTv.ContainsKey(tvdbid))
                    {
                        var dbt = dbTv[tvdbid];

                        viewT.Requested = true;
                        viewT.Approved = dbt.Approved;
                        viewT.Available = dbt.Available;
                    }
                    else if (sonarrCached.Contains(tvdbid) || sickRageCache.Contains(tvdbid)) // compare to the sonarr/sickrage db
                    {
                        viewT.Requested = true;
                    }
                }

                viewTv.Add(viewT);
            }

            Log.Trace("Returning TV Show results: ");
            Log.Trace(viewTv.DumpJson());
            return Response.AsJson(viewTv);
        }

        private Response SearchMusic(string searchTerm)
        {
            var taskList = new List<Task>();

            var apiAlbums = new List<Release>();
            taskList.Add(Task.Factory.StartNew(() =>
            {
                return MusicBrainzApi.SearchAlbum(searchTerm);

            }).ContinueWith((t) =>
            {
                apiAlbums = t.Result.releases ?? new List<Release>();
            }));

            var dbAlbum = new Dictionary<string, RequestedModel>();
            taskList.Add(Task.Factory.StartNew(() =>
            {
                return RequestService.GetAll().Where(x => x.Type == RequestType.Album);

            }).ContinueWith((t) =>
            {
                dbAlbum = t.Result.ToDictionary(x => x.MusicBrainzId);
            }));

            Task.WaitAll(taskList.ToArray());

            var plexAlbums = Checker.GetPlexAlbums();

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
                if (Checker.IsAlbumAvailable(plexAlbums.ToArray(), a.title, release.ToString("yyyy"), artist.name))
                {
                    viewA.Available = true;
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

        private Response RequestMovie(int movieId, bool notify = false)
        {
            var movieApi = new TheMovieDbApi();
            var movieInfo = movieApi.GetMovieInformation(movieId).Result;
            var fullMovieName = $"{movieInfo.Title}{(movieInfo.ReleaseDate.HasValue ? $" ({movieInfo.ReleaseDate.Value.Year})" : string.Empty)}";
            Log.Trace("Getting movie info from TheMovieDb");
            Log.Trace(movieInfo.DumpJson);
            //#if !DEBUG

            var settings = PrService.GetSettings();

            // check if the movie has already been requested
            Log.Info("Requesting movie with id {0}", movieId);
            var existingRequest = RequestService.CheckRequest(movieId);
            if (existingRequest != null)
            {
                // check if the current user is already marked as a requester for this movie, if not, add them
                if (!existingRequest.UserHasRequested(Username))
                {
                    if (notify)
                    {
                        existingRequest.AddUserToNotification(Username);
                    }
                    existingRequest.RequestedUsers.Add(Username);
                    RequestService.UpdateRequest(existingRequest);
                }

                return Response.AsJson(new JsonResponseModel { Result = true, Message = settings.UsersCanViewOnlyOwnRequests ? $"{fullMovieName} was successfully added!" : $"{fullMovieName} has already been requested!" });
            }

            Log.Debug("movie with id {0} doesnt exists", movieId);

            try
            {
                var movies = Checker.GetPlexMovies();
                if (Checker.IsMovieAvailable(movies.ToArray(), movieInfo.Title, movieInfo.ReleaseDate?.Year.ToString()))
                {
                    return Response.AsJson(new JsonResponseModel { Result = false, Message = $"{fullMovieName} is already in Plex!" });
                }
            }
            catch (ApplicationSettingsException)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = $"We could not check if {fullMovieName} is in Plex, are you sure it's correctly setup?" });
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
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                RequestedUsers = new List<string> { Username },
                Issues = IssueState.None,
                
            };

            if (notify)
            {
                model.AddUserToNotification(Username);
            }

            Log.Trace(settings.DumpJson());
            if (ShouldAutoApprove(RequestType.Movie, settings))
            {
                var cpSettings = CpService.GetSettings();

                Log.Trace("Settings: ");
                Log.Trace(cpSettings.DumpJson);
                if (cpSettings.Enabled)
                {
                    Log.Info("Adding movie to CP (No approval required)");
                    var result = CouchPotatoApi.AddMovie(model.ImdbId, cpSettings.ApiKey, model.Title,
                        cpSettings.FullUri, cpSettings.ProfileId);
                    Log.Debug("Adding movie to CP result {0}", result);
                    if (result)
                    {
                        model.Approved = true;
                        Log.Info("Adding movie to database (No approval required)");
                        RequestService.AddRequest(model);

                        var notificationModel = new NotificationModel
                        {
                            Title = model.Title,
                            User = Username,
                            DateTime = DateTime.Now,
                            NotificationType = NotificationType.NewRequest
                        };
                        NotificationService.Publish(notificationModel);

                        return Response.AsJson(new JsonResponseModel { Result = true, Message = $"{fullMovieName} was successfully added!" });
                    }
                    return
                        Response.AsJson(new JsonResponseModel
                        {
                            Result = false,
                            Message =
                                "Something went wrong adding the movie to CouchPotato! Please check your settings."
                        });
                }
                else
                {
                    model.Approved = true;
                    Log.Info("Adding movie to database (No approval required)");
                    RequestService.AddRequest(model);

                    var notificationModel = new NotificationModel
                    {
                        Title = model.Title,
                        User = Username,
                        DateTime = DateTime.Now,
                        NotificationType = NotificationType.NewRequest
                    };
                    NotificationService.Publish(notificationModel);

                    return Response.AsJson(new JsonResponseModel { Result = true, Message = $"{fullMovieName} was successfully added!" });
                }
            }

            try
            {
                Log.Info("Adding movie to database");
                var id = RequestService.AddRequest(model);

                var notificationModel = new NotificationModel { Title = model.Title, User = Username, DateTime = DateTime.Now, NotificationType = NotificationType.NewRequest };
                NotificationService.Publish(notificationModel);

                return Response.AsJson(new JsonResponseModel { Result = true, Message = $"{fullMovieName} was successfully added!" });
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
        /// <param name="seasons">The seasons.</param>
        /// <param name="notify">if set to <c>true</c> [notify].</param>
        /// <returns></returns>
        private Response RequestTvShow(int showId, string seasons, bool notify)
        {
            var tvApi = new TvMazeApi();

            var showInfo = tvApi.ShowLookupByTheTvDbId(showId);
            DateTime firstAir;
            DateTime.TryParse(showInfo.premiered, out firstAir);
            string fullShowName = $"{showInfo.name} ({firstAir.Year})";
            //#if !DEBUG

            var settings = PrService.GetSettings();

            // check if the show has already been requested
            Log.Info("Requesting tv show with id {0}", showId);
            var existingRequest = RequestService.CheckRequest(showId);
            if (existingRequest != null)
            {
                // check if the current user is already marked as a requester for this show, if not, add them
                if (!existingRequest.UserHasRequested(Username))
                {
                    if (notify)
                    {
                        existingRequest.AddUserToNotification(Username);
                    }
                    existingRequest.RequestedUsers.Add(Username);
                    RequestService.UpdateRequest(existingRequest);
                }
                return Response.AsJson(new JsonResponseModel { Result = true, Message = settings.UsersCanViewOnlyOwnRequests ? $"{fullShowName} was successfully added!" : $"{fullShowName} has already been requested!" });
            }

            try
            {
                var shows = Checker.GetPlexTvShows();
                if (Checker.IsTvShowAvailable(shows.ToArray(), showInfo.name, showInfo.premiered?.Substring(0, 4)))
                {
                    return Response.AsJson(new JsonResponseModel { Result = false, Message = $"{fullShowName} is already in Plex!" });
                }
            }
            catch (ApplicationSettingsException)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = $"We could not check if {fullShowName} is in Plex, are you sure it's correctly setup?" });
            }
            //#endif


            var model = new RequestedModel
            {
                ProviderId = showInfo.externals?.thetvdb ?? 0,
                Type = RequestType.TvShow,
                Overview = showInfo.summary.RemoveHtml(),
                PosterPath = showInfo.image?.medium,
                Title = showInfo.name,
                ReleaseDate = firstAir,
                Status = showInfo.status,
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                RequestedUsers = new List<string>() { Username },
                Issues = IssueState.None,
                ImdbId = showInfo.externals?.imdb ?? string.Empty,
                SeasonCount = showInfo.seasonCount
            };
            if (notify)
            {
                model.AddUserToNotification(Username);
            }
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
                default:
                    model.SeasonsRequested = "All";
                    break;
            }

            model.SeasonList = seasonsList.ToArray();

            if (ShouldAutoApprove(RequestType.TvShow, settings))
            {
                var sonarrSettings = SonarrService.GetSettings();
                var sender = new TvSender(SonarrApi, SickrageApi);
                if (sonarrSettings.Enabled)
                {
                    var result = sender.SendToSonarr(sonarrSettings, model);
                    if (result != null && !string.IsNullOrEmpty(result.title))
                    {
                        model.Approved = true;
                        Log.Debug("Adding tv to database requests (No approval required & Sonarr)");
                        RequestService.AddRequest(model);
                        var notify1 = new NotificationModel { Title = model.Title, User = Username, DateTime = DateTime.Now, NotificationType = NotificationType.NewRequest };
                        NotificationService.Publish(notify1);

                        return Response.AsJson(new JsonResponseModel { Result = true, Message = $"{fullShowName} was successfully added!" });
                    }


                    return Response.AsJson(ValidationHelper.SendSonarrError(result?.ErrorMessages));

                }

                var srSettings = SickRageService.GetSettings();
                if (srSettings.Enabled)
                {
                    var result = sender.SendToSickRage(srSettings, model);
                    if (result?.result == "success")
                    {
                        model.Approved = true;
                        Log.Debug("Adding tv to database requests (No approval required & SickRage)");
                        RequestService.AddRequest(model);

                        var notify2 = new NotificationModel { Title = model.Title, User = Username, DateTime = DateTime.Now, NotificationType = NotificationType.NewRequest };
                        NotificationService.Publish(notify2);

                        return Response.AsJson(new JsonResponseModel { Result = true, Message = $"{fullShowName} was successfully added!" });
                    }
                    return Response.AsJson(new JsonResponseModel { Result = false, Message = result?.message != null ? "<b>Message From SickRage: </b>" + result.message : "Something went wrong adding the movie to SickRage! Please check your settings." });
                }

                return Response.AsJson(new JsonResponseModel { Result=false, Message = "The request of TV Shows is not correctly set up. Please contact your admin."});

            }

            RequestService.AddRequest(model);

            var notificationModel = new NotificationModel { Title = model.Title, User = Username, DateTime = DateTime.Now, NotificationType = NotificationType.NewRequest };
            NotificationService.Publish(notificationModel);

            return Response.AsJson(new JsonResponseModel { Result = true, Message = $"{fullShowName} was successfully added!" });
        }

        private Response RequestAlbum(string releaseId, bool notify)
        {
            var settings = PrService.GetSettings();
            var existingRequest = RequestService.CheckRequest(releaseId);
            Log.Debug("Checking for an existing request");

            if (existingRequest != null)
            {
                Log.Debug("We do have an existing album request");
                if (!existingRequest.UserHasRequested(Username))
                {
                    if (notify)
                    {
                        existingRequest.AddUserToNotification(Username);
                    }
                    Log.Debug("Not in the requested list so adding them and updating the request. User: {0}", Username);
                    existingRequest.RequestedUsers.Add(Username);
                    RequestService.UpdateRequest(existingRequest);
                }
                return Response.AsJson(new JsonResponseModel { Result = true, Message = settings.UsersCanViewOnlyOwnRequests ? $"{existingRequest.Title} was successfully added!" : $"{existingRequest.Title} has already been requested!" });
            }


            Log.Debug("This is a new request");

            var albumInfo = MusicBrainzApi.GetAlbum(releaseId);
            DateTime release;
            DateTimeHelper.CustomParse(albumInfo.ReleaseEvents?.FirstOrDefault()?.date, out release);

            var artist = albumInfo.ArtistCredits?.FirstOrDefault()?.artist;
            if (artist == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "We could not find the artist on MusicBrainz. Please try again later or contact your admin" });
            }

            var albums = Checker.GetPlexAlbums();
            var alreadyInPlex = Checker.IsAlbumAvailable(albums.ToArray(), albumInfo.title, release.ToString("yyyy"), artist.name);

            if (alreadyInPlex)
            {
                return Response.AsJson(new JsonResponseModel
                {
                    Result = false,
                    Message = $"{albumInfo.title} is already in Plex!"
                });
            }

            var img = GetMusicBrainzCoverArt(albumInfo.id);

            Log.Trace("Album Details:");
            Log.Trace(albumInfo.DumpJson());
            Log.Trace("CoverArt Details:");
            Log.Trace(img.DumpJson());


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

            if (notify)
            {
                model.AddUserToNotification(Username);
            }
            if (ShouldAutoApprove(RequestType.Album, settings))
            {
                Log.Debug("We don't require approval OR the user is in the whitelist");
                var hpSettings = HeadphonesService.GetSettings();

                Log.Trace("Headphone Settings:");
                Log.Trace(hpSettings.DumpJson());

                if (!hpSettings.Enabled)
                {
                    RequestService.AddRequest(model);
                    return
                        Response.AsJson(new JsonResponseModel
                        {
                            Result = true,
                            Message = $"{model.Title} was successfully added!"
                        });
                }

                var sender = new HeadphonesSender(HeadphonesApi, hpSettings, RequestService);
                sender.AddAlbum(model);
                model.Approved = true;
                RequestService.AddRequest(model);

                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = true,
                        Message = $"{model.Title} was successfully added!"
                    });
            }

            var result = RequestService.AddRequest(model);
            return Response.AsJson(new JsonResponseModel
            {
                Result = true,
                Message = $"{model.Title} was successfully added!"
            });
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

        private bool ShouldAutoApprove(RequestType requestType, PlexRequestSettings prSettings)
        {
            // if the user is an admin or they are whitelisted, they go ahead and allow auto-approval
            if (IsAdmin || prSettings.ApprovalWhiteList.Any(x => x.Equals(Username, StringComparison.OrdinalIgnoreCase))) return true;

            // check by request type if the category requires approval or not
            switch (requestType)
            {
                case RequestType.Movie:
                    return !prSettings.RequireMovieApproval;
                case RequestType.TvShow:
                    return !prSettings.RequireTvShowApproval;
                case RequestType.Album:
                    return !prSettings.RequireMusicApproval;
                default:
                    return false;
            }
        }
    }
}
