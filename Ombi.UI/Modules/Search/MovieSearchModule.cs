using Nancy;
using Nancy.Extensions;
using Ombi.Api;
using Ombi.Api.Interfaces;
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Helpers.Analytics;
using Ombi.Helpers.Permissions;
using Ombi.Services.Interfaces;
using Ombi.Services.Jobs;
using Ombi.Services.Notification;
using Ombi.Store;
using Ombi.Store.Models;
using Ombi.Store.Models.Emby;
using Ombi.Store.Repository;
using Ombi.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Objects.General;
using Ombi.Core.Queue;
using Action = Ombi.Helpers.Analytics.Action;
using Ombi.Store.Models.Plex;

namespace Ombi.UI.Modules.Search
{
    public class MovieSearchModule : BaseSearchModule
    {
        protected TheMovieDbApi MovieApi { get; }
        protected ICouchPotatoCacher CpCacher { get; }
        protected IRadarrCacher RadarrCacher { get; }
        protected IMovieSender MovieSender { get; }
        protected IWatcherCacher WatcherCacher { get; }

        public MovieSearchModule(ICouchPotatoCacher cpCacher, IMovieSender movieSender,
            IRadarrCacher radarrCacher, IWatcherCacher watcherCacher,
            /* parameters needed for base class constructor */
            IPlexApi plexApi, ISettingsService<PlexRequestSettings> prSettings,
            ISettingsService<PlexSettings> plexService, ISettingsService<AuthenticationSettings> auth,
            ISecurityExtensions security, IAvailabilityChecker plexChecker, INotificationService notify,
            ISettingsService<CustomizationSettings> cus, IRequestService request, IAnalytics a,
            IRepository<UsersToNotify> u, ISettingsService<EmailNotificationSettings> email,
            IIssueService issue, IRepository<RequestLimit> rl, ITransientFaultQueue tfQueue,
            IRepository<PlexContent> content, IEmbyAvailabilityChecker embyChecker,
            IRepository<EmbyContent> embyContent, ISettingsService<EmbySettings> embySettings)
            : base(plexApi, prSettings, plexService, auth, security, plexChecker, notify, cus, request, a, u,
                  email, issue, rl, tfQueue, content, embyChecker, embyContent, embySettings)
        {
            MovieApi = new TheMovieDbApi();
            CpCacher = cpCacher;
            MovieSender = movieSender;
            RadarrCacher = radarrCacher;
            WatcherCacher = watcherCacher;

            Get["movie/{searchTerm}", true] = async (x, ct) => await SearchMovie((string)x.searchTerm);
            Get["movie/upcoming", true] = async (x, ct) => await UpcomingMovies();
            Get["movie/playing", true] = async (x, ct) => await CurrentlyPlayingMovies();
            Post["request/movie", true] = async (x, ct) => await RequestMovie((int)Request.Form.movieId);
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
            var watcherCached = WatcherCacher.QueuedIds();
            var radarrCached = RadarrCacher.QueuedIds();

            var viewMovies = new List<SearchMovieViewModel>();
            var counter = 0;
            foreach (var movie in apiMovies)
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

                if (counter <= 5) // Let's only do it for the first 5 items
                {
                    var movieInfo = MovieApi.GetMovieInformationWithVideos(movie.Id);

                    // TODO needs to be careful about this, it's adding extra time to search...
                    // https://www.themoviedb.org/talk/5807f4cdc3a36812160041f2
                    viewMovie.ImdbId = movieInfo?.imdb_id;
                    viewMovie.Homepage = movieInfo?.homepage;
                    var videoId = movieInfo?.video ?? false
                        ? movieInfo?.videos?.results?.FirstOrDefault()?.key
                        : string.Empty;

                    viewMovie.Trailer = string.IsNullOrEmpty(videoId)
                        ? string.Empty
                        : $"https://www.youtube.com/watch?v={videoId}";

                    counter++;
                }

                var canSee = CanUserSeeThisRequest(viewMovie.Id, Security.HasPermissions(User, Permissions.UsersCanViewOnlyOwnRequests), dbMovies);

                var plexSettings = await PlexService.GetSettingsAsync();
                var embySettings = await EmbySettings.GetSettingsAsync();
                if (plexSettings.Enable)
                {
                    var content = PlexContentRepository.GetAll();
                    var plexMovies = PlexChecker.GetPlexMovies(content);

                    var plexMovie = PlexChecker.GetMovie(plexMovies.ToArray(), movie.Title,
                        movie.ReleaseDate?.Year.ToString(),
                        viewMovie.ImdbId);
                    if (plexMovie != null)
                    {
                        viewMovie.Available = true;
                        viewMovie.PlexUrl = plexMovie.Url;
                    }
                }
                if (embySettings.Enable)
                {
                    var embyContent = EmbyContentRepository.GetAll();
                    var embyMovies = EmbyChecker.GetEmbyMovies(embyContent);

                    var embyMovie = EmbyChecker.GetMovie(embyMovies.ToArray(), movie.Title,
                        movie.ReleaseDate?.Year.ToString(), viewMovie.ImdbId);
                    if (embyMovie != null)
                    {
                        viewMovie.Available = true;
                    }
                }
                if (dbMovies.ContainsKey(movie.Id) && canSee) // compare to the requests db
                {
                    var dbm = dbMovies[movie.Id];

                    viewMovie.Requested = true;
                    viewMovie.Approved = dbm.Approved;
                    viewMovie.Available = dbm.Available;
                }
                if (cpCached.Contains(movie.Id) && canSee) // compare to the couchpotato db
                {
                    viewMovie.Approved = true;
                    viewMovie.Requested = true;
                }
                if (watcherCached.Contains(viewMovie.ImdbId) && canSee) // compare to the watcher db
                {
                    viewMovie.Approved = true;
                    viewMovie.Requested = true;
                }
                if (radarrCached.Contains(movie.Id) && canSee)
                {
                    viewMovie.Approved = true;
                    viewMovie.Requested = true;
                }
                viewMovies.Add(viewMovie);
            }

            return Response.AsJson(viewMovies);
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
            var embySettings = await EmbySettings.GetSettingsAsync();
            Analytics.TrackEventAsync(Category.Search, Action.Request, "Movie", Username,
                CookieHelper.GetAnalyticClientId(Cookies));
            var movieInfo = await MovieApi.GetMovieInformation(movieId);
            if (movieInfo == null)
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = "There was an issue adding this movie!"
                    });
            }
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
                            Security.HasPermissions(User, Permissions.UsersCanViewOnlyOwnRequests)
                                ? $"{fullMovieName} {Ombi.UI.Resources.UI.Search_SuccessfullyAdded}"
                                : $"{fullMovieName} {Resources.UI.Search_AlreadyRequested}"
                    });
            }

            try
            {

                var content = PlexContentRepository.GetAll();
                var movies = PlexChecker.GetPlexMovies(content);
                if (PlexChecker.IsMovieAvailable(movies.ToArray(), movieInfo.Title, movieInfo.ReleaseDate?.Year.ToString()))
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
                        Message = string.Format(Resources.UI.Search_CouldNotCheckPlex, fullMovieName, GetMediaServerName())
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
                if (ShouldAutoApprove(RequestType.Movie))
                {
                    model.Approved = true;

                    var result = await MovieSender.Send(model);
                    if (result.Result)
                    {
                        return await AddRequest(model, settings,
                            $"{fullMovieName} {Resources.UI.Search_SuccessfullyAdded}");
                    }
                    if (result.Error)

                    {
                        return
                            Response.AsJson(new JsonResponseModel
                            {
                                Message = "Could not add movie, please contract your administrator",
                                Result = false
                            });
                    }
                    if (!result.MovieSendingEnabled)
                    {

                        return await AddRequest(model, settings, $"{fullMovieName} {Resources.UI.Search_SuccessfullyAdded}");
                    }

                    return Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = Resources.UI.Search_CouchPotatoError
                    });
                }


                return await AddRequest(model, settings, $"{fullMovieName} {Resources.UI.Search_SuccessfullyAdded}");
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                await FaultQueue.QueueItemAsync(model, movieInfo.Id.ToString(), RequestType.Movie, FaultType.RequestFault, e.Message);

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


    }
}
