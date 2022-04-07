using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Helpers;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;
using Ombi.Core.Models.UI;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Core.Engine.V2
{
    public class MovieSearchEngineV2 : BaseMediaEngine, IMovieEngineV2
    {
        public MovieSearchEngineV2(ICurrentUser identity, IRequestServiceMain service, IMovieDbApi movApi, IMapper mapper,
            ILogger<MovieSearchEngineV2> logger, IRuleEvaluator r, OmbiUserManager um, ICacheService mem, ISettingsService<OmbiSettings> s, IRepository<RequestSubscription> sub,
            ISettingsService<CustomizationSettings> customizationSettings, IMovieRequestEngine movieRequestEngine, IHttpClientFactory httpClientFactory)
            : base(identity, service, r, um, mem, s, sub)
        {
            MovieApi = movApi;
            Mapper = mapper;
            Logger = logger;
            _customizationSettings = customizationSettings;
            _movieRequestEngine = movieRequestEngine;
            _client = httpClientFactory.CreateClient();
        }

        private IMovieDbApi MovieApi { get; }
        private IMapper Mapper { get; }
        private ILogger Logger { get; }
        private readonly ISettingsService<CustomizationSettings> _customizationSettings;
        private readonly IMovieRequestEngine _movieRequestEngine;
        private readonly HttpClient _client;

        public async Task<MovieFullInfoViewModel> GetFullMovieInformation(int theMovieDbId, CancellationToken cancellationToken, string langCode = null)
        {
            langCode = await DefaultLanguageCode(langCode);
            var movieInfo = await Cache.GetOrAddAsync(nameof(GetFullMovieInformation) + theMovieDbId + langCode,
                () =>  MovieApi.GetFullMovieInfo(theMovieDbId, cancellationToken, langCode), DateTimeOffset.Now.AddHours(12));

            return await ProcessSingleMovie(movieInfo);
        }

        public async Task<MovieFullInfoViewModel> GetMovieInfoByRequestId(int requestId, CancellationToken cancellationToken, string langCode = null)
        {
            langCode = await DefaultLanguageCode(langCode);
            var request = await RequestService.MovieRequestService.Find(requestId);
            var movieInfo = await Cache.GetOrAddAsync(nameof(GetFullMovieInformation) + request.TheMovieDbId + langCode,
                () =>  MovieApi.GetFullMovieInfo(request.TheMovieDbId, cancellationToken, langCode), DateTimeOffset.Now.AddHours(12));

            return await ProcessSingleMovie(movieInfo);
        }

        public async Task<MovieCollectionsViewModel> GetCollection(int collectionId, CancellationToken cancellationToken, string langCode = null)
        {
            langCode = await DefaultLanguageCode(langCode);
            var collections = await Cache.GetOrAddAsync(nameof(GetCollection) + collectionId + langCode,
                () =>  MovieApi.GetCollection(langCode, collectionId, cancellationToken), DateTimeOffset.Now.AddDays(1));

            var c = await ProcessCollection(collections);
            c.Collection = c.Collection.OrderBy(x => x.ReleaseDate).ToList();
            return c;
        }

        public async Task<int> GetTvDbId(int theMovieDbId)
        {
            var result = await MovieApi.GetTvExternals(theMovieDbId);
            return result.tvdb_id;
        }

        /// <summary>
        /// Get similar movies to the id passed in
        /// </summary>
        /// <param name="theMovieDbId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> SimilarMovies(int theMovieDbId, string langCode)
        {
            langCode = await DefaultLanguageCode(langCode);
            var result = await MovieApi.SimilarMovies(theMovieDbId, langCode);
            if (result != null)
            {
                Logger.LogDebug("Search Result: {result}", result);
                return await TransformMovieResultsToResponse(result.Shuffle().Take(ResultLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        /// <summary>
        /// Gets popular movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> PopularMovies()
        {

            var result = await Cache.GetOrAddAsync(CacheKeys.PopularMovies, async () =>
            {
                var langCode = await DefaultLanguageCode(null);
                return await MovieApi.PopularMovies(langCode);
            }, DateTimeOffset.Now.AddHours(12));
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.Shuffle().Take(ResultLimit)); // Take x to stop us overloading the API
            }
            return null;
        }


        private const int _theMovieDbMaxPageItems = 20;

        /// <summary>
        /// Gets popular movies by paging
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> PopularMovies(int currentlyLoaded, int toLoad, CancellationToken cancellationToken, string langCustomCode = null)
        {
            var langCode = await DefaultLanguageCode(langCustomCode);

            var pages = PaginationHelper.GetNextPages(currentlyLoaded, toLoad, _theMovieDbMaxPageItems);

            var results = new List<MovieDbSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAddAsync(nameof(PopularMovies) + pagesToLoad.Page + langCode,
                    () => MovieApi.PopularMovies(langCode, pagesToLoad.Page, cancellationToken), DateTimeOffset.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }
            return await TransformMovieResultsToResponse(results);
        }

        public async Task<IEnumerable<SearchMovieViewModel>> AdvancedSearch(DiscoverModel model, int currentlyLoaded, int toLoad, CancellationToken cancellationToken)
        {
            var langCode = await DefaultLanguageCode(null);

            //var pages = PaginationHelper.GetNextPages(currentlyLoaded, toLoad, _theMovieDbMaxPageItems);

            var results = new List<MovieDbSearchResult>();
            //foreach (var pagesToLoad in pages)
            //{
                var apiResult = await MovieApi.AdvancedSearch(model, cancellationToken);
                //results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            //}
            return await TransformMovieResultsToResponse(apiResult);
        }

        /// <summary>
        /// Gets top rated movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies()
        {
            var result = await Cache.GetOrAddAsync(CacheKeys.TopRatedMovies, async () =>
            {
                var langCode = await DefaultLanguageCode(null);
                return await MovieApi.TopRated(langCode);
            }, DateTimeOffset.Now.AddHours(12));
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.Shuffle().Take(ResultLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies(int currentPosition, int amountToLoad)
        {
            var langCode = await DefaultLanguageCode(null);

            var pages = PaginationHelper.GetNextPages(currentPosition, amountToLoad, _theMovieDbMaxPageItems);

            var results = new List<MovieDbSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAddAsync(nameof(TopRatedMovies) + pagesToLoad.Page + langCode,
                    () =>  MovieApi.TopRated(langCode, pagesToLoad.Page), DateTimeOffset.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }
            return await TransformMovieResultsToResponse(results);
        }

        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies(int currentPosition, int amountToLoad)
        {
            var langCode = await DefaultLanguageCode(null);

            var pages = PaginationHelper.GetNextPages(currentPosition, amountToLoad, _theMovieDbMaxPageItems);

            var results = new List<MovieDbSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAddAsync(nameof(NowPlayingMovies) + pagesToLoad.Page + langCode,
                    () =>  MovieApi.NowPlaying(langCode, pagesToLoad.Page), DateTimeOffset.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }
            return await TransformMovieResultsToResponse(results);
        }

        public async Task<IEnumerable<SearchMovieViewModel>> SeasonalList(int currentPosition, int amountToLoad, CancellationToken cancellationToken)
        {
            var langCode = await DefaultLanguageCode(null);

            var result = await _client.GetAsync("https://raw.githubusercontent.com/Ombi-app/Ombi.News/main/Seasonal.md");
            var keyWordIds = await result.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(keyWordIds) || keyWordIds.Equals("\n"))
            {
                return new List<SearchMovieViewModel>();
            }

            var pages = PaginationHelper.GetNextPages(currentPosition, amountToLoad, _theMovieDbMaxPageItems);

            var results = new List<MovieDbSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAddAsync(nameof(SeasonalList) + pagesToLoad.Page + langCode + keyWordIds,
                    () =>  MovieApi.GetMoviesViaKeywords(keyWordIds, langCode, cancellationToken, pagesToLoad.Page), DateTimeOffset.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }
            return await TransformMovieResultsToResponse(results);
        }

        /// <summary>
        /// Gets recently requested movies
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> RecentlyRequestedMovies(int currentlyLoaded, int toLoad, CancellationToken cancellationToken)
        {
            var langCode = await DefaultLanguageCode(null);

            var results = new List<MovieResponseDto>();

            var requestResult = await Cache.GetOrAddAsync(nameof(RecentlyRequestedMovies) + "Requests" + toLoad + langCode,
                async () =>
                {
                    return await _movieRequestEngine.GetRequests(toLoad, currentlyLoaded, new Models.UI.OrderFilterModel
                    {
                        OrderType = OrderType.RequestedDateDesc
                    });
                }, DateTimeOffset.Now.AddMinutes(15));

            var movieDBResults = await Cache.GetOrAddAsync(nameof(RecentlyRequestedMovies) + toLoad + langCode,
                async () =>
                {
                    var responses = new List<MovieResponseDto>();
                    foreach(var movie in requestResult.Collection)
                    {
                        responses.Add(await MovieApi.GetMovieInformation(movie.TheMovieDbId));
                    }
                    return responses;
                }, DateTimeOffset.Now.AddHours(12));

            results.AddRange(movieDBResults);

            return await TransformMovieResultsToResponse(results);
        }


        /// <summary>
        /// Gets upcoming movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies()
        {
            var result = await Cache.GetOrAddAsync(CacheKeys.UpcomingMovies, async () =>
            {
                var langCode = await DefaultLanguageCode(null);
                return await MovieApi.Upcoming(langCode);
            }, DateTimeOffset.Now.AddHours(12));
            if (result != null)
            {
                Logger.LogDebug("Search Result: {result}", result);
                return await TransformMovieResultsToResponse(result.Shuffle().Take(ResultLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies(int currentPosition, int amountToLoad)
        {
            var langCode = await DefaultLanguageCode(null);

            var pages = PaginationHelper.GetNextPages(currentPosition, amountToLoad, _theMovieDbMaxPageItems);

            var results = new List<MovieDbSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAddAsync(nameof(UpcomingMovies) + pagesToLoad.Page + langCode,
                    () =>  MovieApi.Upcoming(langCode, pagesToLoad.Page), DateTimeOffset.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }
            return await TransformMovieResultsToResponse(results);
        }

        /// <summary>
        /// Gets now playing movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies()
        {
            var result = await Cache.GetOrAddAsync(CacheKeys.NowPlayingMovies, async () =>
            {
                var langCode = await DefaultLanguageCode(null);
                return await MovieApi.NowPlaying(langCode);
            }, DateTimeOffset.Now.AddHours(12));
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.Shuffle().Take(ResultLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        public async Task<ActorCredits> GetMoviesByActor(int actorId, string langCode)
        {
            langCode = await DefaultLanguageCode(langCode);
            var result = await Cache.GetOrAddAsync(nameof(GetMoviesByActor) + actorId + langCode,
                () =>  MovieApi.GetActorMovieCredits(actorId, langCode), DateTimeOffset.Now.AddHours(12));
            // Later we run this through the rules engine
            return result;
        }

        public async Task<IEnumerable<StreamingData>> GetStreamInformation(int movieDbId, CancellationToken cancellationToken)
        {
            var providers = await MovieApi.GetMovieWatchProviders(movieDbId, cancellationToken);
            var results = await GetUserWatchProvider(providers);

            var data = new List<StreamingData>();

            foreach (var result in results)
            {
                data.Add(new StreamingData
                {
                    Logo = result.logo_path,
                    Order = result.display_priority,
                    StreamingProvider = result.provider_name
                });
            }

            return data;
        }

        protected async Task<List<SearchMovieViewModel>> TransformMovieResultsToResponse<T>(
            IEnumerable<T> movies) where T: new()
        {
            var settings = await _customizationSettings.GetSettingsAsync();
            var viewMovies = new List<SearchMovieViewModel>();
            foreach (var movie in movies)
            {
                var result = await ProcessSingleMovie(movie);

                if (DemoCheck(result.Title))
                {
                    continue;
                }

                if (settings.HideAvailableFromDiscover && result.Available)
                {
                    continue;
                }
                viewMovies.Add(result);
            }
            return viewMovies;
        }

        private async Task<SearchMovieViewModel> ProcessSingleMovie<T>(T movie) where T : new()
        {
            var viewMovie = Mapper.Map<SearchMovieViewModel>(movie);
            return await ProcessSingleMovie(viewMovie);
        }

        private async Task<MovieFullInfoViewModel> ProcessSingleMovie(FullMovieInfo movie)
        {
            var viewMovie = Mapper.Map<SearchMovieViewModel>(movie);
            var user = await GetUser();
            var digitalReleaseDate = viewMovie.ReleaseDates?.Results?.FirstOrDefault(x => x.IsoCode == user.StreamingCountry);
            if (digitalReleaseDate == null)
            {
                digitalReleaseDate = viewMovie.ReleaseDates?.Results?.FirstOrDefault(x => x.IsoCode == "US");
            }
            viewMovie.DigitalReleaseDate = digitalReleaseDate?.ReleaseDate?.FirstOrDefault(x => x.Type == ReleaseDateType.Digital)?.ReleaseDate;

            await RunSearchRules(viewMovie);

            // This requires the rules to be run first to populate the RequestId property
            await CheckForSubscription(viewMovie);
            var mapped = Mapper.Map<MovieFullInfoViewModel>(movie);

            mapped.Available = viewMovie.Available;
            mapped.Approved = viewMovie.Approved;
            mapped.RequestId = viewMovie.RequestId;
            mapped.Requested = viewMovie.Requested;
            mapped.PlexUrl = viewMovie.PlexUrl;
            mapped.EmbyUrl = viewMovie.EmbyUrl;
            mapped.JellyfinUrl = viewMovie.JellyfinUrl;
            mapped.Subscribed = viewMovie.Subscribed;
            mapped.ShowSubscribe = viewMovie.ShowSubscribe;
            mapped.DigitalReleaseDate = viewMovie.DigitalReleaseDate;
            mapped.RequestedDate4k = viewMovie.RequestedDate4k;
            mapped.Approved4K = viewMovie.Approved4K;
            mapped.Available4K = viewMovie.Available4K;
            mapped.Denied4K = viewMovie.Denied4K;
            mapped.DeniedReason4K = viewMovie.DeniedReason4K;
            mapped.Has4KRequest = viewMovie.Has4KRequest;
            

            return mapped;
        }


        private async Task<MovieCollectionsViewModel> ProcessCollection(Collections collection)
        {
            var viewMovie = Mapper.Map<MovieCollectionsViewModel>(collection);
            foreach (var movie in viewMovie.Collection)
            {
                var mappedMovie = Mapper.Map<SearchMovieViewModel>(movie);
                await RunSearchRules(mappedMovie);

                // This requires the rules to be run first to populate the RequestId property
                await CheckForSubscription(mappedMovie);
                var mapped = Mapper.Map<MovieCollection>(movie);

                mapped.Available = movie.Available;
                mapped.Approved = movie.Approved;
                mapped.RequestId = movie.RequestId;
                mapped.Requested = movie.Requested;
                mapped.PlexUrl = movie.PlexUrl;
                mapped.EmbyUrl = movie.EmbyUrl;
                mapped.JellyfinUrl = movie.JellyfinUrl;
                mapped.Subscribed = movie.Subscribed;
                mapped.ShowSubscribe = movie.ShowSubscribe;
                mapped.ReleaseDate = movie.ReleaseDate;
            }
            return viewMovie;
        }

        private async Task<SearchMovieViewModel> ProcessSingleMovie(SearchMovieViewModel viewMovie)
        {
            if (viewMovie.ImdbId.IsNullOrEmpty())
            {
                var showInfo = await Cache.GetOrAddAsync("GetMovieInformationWIthImdbId" + viewMovie.Id,
                    () =>  MovieApi.GetMovieInformation(viewMovie.Id), DateTimeOffset.Now.AddHours(12));
                viewMovie.Id = showInfo.Id; // TheMovieDbId
                viewMovie.ImdbId = showInfo.ImdbId;
            }

            var user = await GetUser();
            var digitalReleaseDate = viewMovie.ReleaseDates?.Results?.FirstOrDefault(x => x.IsoCode == user.StreamingCountry);
            if (digitalReleaseDate == null)
            {
                digitalReleaseDate = viewMovie.ReleaseDates?.Results?.FirstOrDefault(x => x.IsoCode == "US");
            }
            viewMovie.DigitalReleaseDate = digitalReleaseDate?.ReleaseDate?.FirstOrDefault(x => x.Type == ReleaseDateType.Digital)?.ReleaseDate;


            viewMovie.TheMovieDbId = viewMovie.Id.ToString();

            await RunSearchRules(viewMovie);

            // This requires the rules to be run first to populate the RequestId property
            await CheckForSubscription(viewMovie);

            return viewMovie;
        }

        private async Task CheckForSubscription(SearchViewModel viewModel)
        {
            // Check if this user requested it
            var user = await GetUser();
            if (user == null)
            {
                return;
            }
            var request = await RequestService.MovieRequestService.GetAll()
                .AnyAsync(x => x.RequestedUserId.Equals(user.Id) && x.TheMovieDbId == viewModel.Id);
            if (request)
            {
                viewModel.ShowSubscribe = false;
            }
            else
            {
                viewModel.ShowSubscribe = true;
                var sub = await _subscriptionRepository.GetAll().FirstOrDefaultAsync(s => s.UserId == user.Id
                                                                                          && s.RequestId == viewModel.RequestId && s.RequestType == RequestType.Movie);
                viewModel.Subscribed = sub != null;
            }
        }

        public async Task<MovieFullInfoViewModel> GetMovieInfoByImdbId(string imdbId, CancellationToken cancellationToken)
        {
            var langCode = await DefaultLanguageCode(null);
            var findResult = await Cache.GetOrAddAsync(nameof(GetMovieInfoByImdbId) + imdbId + langCode,
                () =>  MovieApi.Find(imdbId, ExternalSource.imdb_id), DateTimeOffset.Now.AddHours(12));

            var movie = findResult.movie_results.FirstOrDefault();
            var movieInfo = await Cache.GetOrAddAsync(nameof(GetMovieInfoByImdbId) + movie.id + langCode,
                () =>  MovieApi.GetFullMovieInfo(movie.id, cancellationToken, langCode), DateTimeOffset.Now.AddHours(12));

            return await ProcessSingleMovie(movieInfo);
        }
    }
}