using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Core.Engine.V2
{
    public class MovieSearchEngineV2 : BaseMediaEngine, IMovieEngineV2
    {
        public MovieSearchEngineV2(IPrincipal identity, IRequestServiceMain service, IMovieDbApi movApi, IMapper mapper,
            ILogger<MovieSearchEngineV2> logger, IRuleEvaluator r, OmbiUserManager um, ICacheService mem, ISettingsService<OmbiSettings> s, IRepository<RequestSubscription> sub,
            ISettingsService<CustomizationSettings> customizationSettings)
            : base(identity, service, r, um, mem, s, sub)
        {
            MovieApi = movApi;
            Mapper = mapper;
            Logger = logger;
            _customizationSettings = customizationSettings;
        }

        private IMovieDbApi MovieApi { get; }
        private IMapper Mapper { get; }
        private ILogger Logger { get; }
        private readonly ISettingsService<CustomizationSettings> _customizationSettings;


        public async Task<MovieFullInfoViewModel> GetFullMovieInformation(int theMovieDbId, CancellationToken cancellationToken, string langCode = null)
        {
            langCode = await DefaultLanguageCode(langCode);
            var movieInfo = await Cache.GetOrAdd(nameof(GetFullMovieInformation) + theMovieDbId + langCode,
                async () => await MovieApi.GetFullMovieInfo(theMovieDbId, cancellationToken, langCode), DateTime.Now.AddHours(12), cancellationToken);

            return await ProcessSingleMovie(movieInfo);
        }

        public async Task<MovieFullInfoViewModel> GetMovieInfoByRequestId(int requestId, CancellationToken cancellationToken, string langCode = null)
        {
            langCode = await DefaultLanguageCode(langCode);
            var request = await RequestService.MovieRequestService.Find(requestId);
            var movieInfo = await Cache.GetOrAdd(nameof(GetFullMovieInformation) + request.TheMovieDbId + langCode,
                async () => await MovieApi.GetFullMovieInfo(request.TheMovieDbId, cancellationToken, langCode), DateTime.Now.AddHours(12), cancellationToken);

            return await ProcessSingleMovie(movieInfo);
        }

        public async Task<MovieCollectionsViewModel> GetCollection(int collectionId, CancellationToken cancellationToken, string langCode = null)
        {
            langCode = await DefaultLanguageCode(langCode);
            var collections = await Cache.GetOrAdd(nameof(GetCollection) + collectionId + langCode,
                async () => await MovieApi.GetCollection(langCode, collectionId, cancellationToken), DateTime.Now.AddDays(1), cancellationToken);

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

            var result = await Cache.GetOrAdd(CacheKeys.PopularMovies, async () =>
            {
                var langCode = await DefaultLanguageCode(null);
                return await MovieApi.PopularMovies(langCode);
            }, DateTime.Now.AddHours(12));
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
        public async Task<IEnumerable<SearchMovieViewModel>> PopularMovies(int currentlyLoaded, int toLoad, CancellationToken cancellationToken)
        {
            var langCode = await DefaultLanguageCode(null);

            var pages = PaginationHelper.GetNextPages(currentlyLoaded, toLoad, _theMovieDbMaxPageItems);

            var results = new List<MovieSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAdd(nameof(PopularMovies) + pagesToLoad.Page + langCode,
                    async () => await MovieApi.PopularMovies(langCode, pagesToLoad.Page, cancellationToken), DateTime.Now.AddHours(12), cancellationToken);
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }
            return await TransformMovieResultsToResponse(results);
        }

        /// <summary>
        /// Gets top rated movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies()
        {
            var result = await Cache.GetOrAdd(CacheKeys.TopRatedMovies, async () =>
            {
                var langCode = await DefaultLanguageCode(null);
                return await MovieApi.TopRated(langCode);
            }, DateTime.Now.AddHours(12));
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

            var results = new List<MovieSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAdd(nameof(TopRatedMovies) + pagesToLoad.Page + langCode,
                    async () => await MovieApi.TopRated(langCode, pagesToLoad.Page), DateTime.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }
            return await TransformMovieResultsToResponse(results);
        }

        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies(int currentPosition, int amountToLoad)
        {
            var langCode = await DefaultLanguageCode(null);

            var pages = PaginationHelper.GetNextPages(currentPosition, amountToLoad, _theMovieDbMaxPageItems);

            var results = new List<MovieSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAdd(nameof(NowPlayingMovies) + pagesToLoad.Page + langCode,
                    async () => await MovieApi.NowPlaying(langCode, pagesToLoad.Page), DateTime.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }
            return await TransformMovieResultsToResponse(results);
        }


        /// <summary>
        /// Gets upcoming movies.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies()
        {
            var result = await Cache.GetOrAdd(CacheKeys.UpcomingMovies, async () =>
            {
                var langCode = await DefaultLanguageCode(null);
                return await MovieApi.Upcoming(langCode);
            }, DateTime.Now.AddHours(12));
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

            var results = new List<MovieSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAdd(nameof(UpcomingMovies) + pagesToLoad.Page + langCode,
                    async () => await MovieApi.Upcoming(langCode, pagesToLoad.Page), DateTime.Now.AddHours(12));
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
            var result = await Cache.GetOrAdd(CacheKeys.NowPlayingMovies, async () =>
            {
                var langCode = await DefaultLanguageCode(null);
                return await MovieApi.NowPlaying(langCode);
            }, DateTime.Now.AddHours(12));
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.Shuffle().Take(ResultLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        public async Task<ActorCredits> GetMoviesByActor(int actorId, string langCode)
        {
            var result = await Cache.GetOrAdd(nameof(GetMoviesByActor) + actorId + langCode,
                async () => await MovieApi.GetActorMovieCredits(actorId, langCode));
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

        protected async Task<List<SearchMovieViewModel>> TransformMovieResultsToResponse(
            IEnumerable<MovieSearchResult> movies)
        {
            var settings = await _customizationSettings.GetSettingsAsync();
            var viewMovies = new List<SearchMovieViewModel>();
            foreach (var movie in movies)
            {
                var result = await ProcessSingleMovie(movie);
                if (settings.HideAvailableFromDiscover && result.Available)
                {
                    continue;
                }
                viewMovies.Add(result);
            }
            return viewMovies;
        }

        private async Task<SearchMovieViewModel> ProcessSingleMovie(MovieSearchResult movie)
        {
            var viewMovie = Mapper.Map<SearchMovieViewModel>(movie);
            return await ProcessSingleMovie(viewMovie);
        }

        private async Task<MovieFullInfoViewModel> ProcessSingleMovie(FullMovieInfo movie)
        {
            var viewMovie = Mapper.Map<SearchMovieViewModel>(movie);
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
                var showInfo = await Cache.GetOrAdd("GetMovieInformationWIthImdbId" + viewMovie.Id,
                    async () => await MovieApi.GetMovieInformation(viewMovie.Id), DateTime.Now.AddHours(12));
                viewMovie.Id = showInfo.Id; // TheMovieDbId
                viewMovie.ImdbId = showInfo.ImdbId;
            }

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
            var findResult = await Cache.GetOrAdd(nameof(GetMovieInfoByImdbId) + imdbId + langCode,
                async () => await MovieApi.Find(imdbId, ExternalSource.imdb_id), DateTime.Now.AddHours(12), cancellationToken);

            var movie = findResult.movie_results.FirstOrDefault();
            var movieInfo = await Cache.GetOrAdd(nameof(GetMovieInfoByImdbId) + movie.id + langCode,
                async () => await MovieApi.GetFullMovieInfo(movie.id, cancellationToken, langCode), DateTime.Now.AddHours(12), cancellationToken);

            return await ProcessSingleMovie(movieInfo);
        }
    }
}