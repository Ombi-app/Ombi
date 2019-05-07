using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
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
using System.Threading.Tasks;

namespace Ombi.Core.Engine
{
    public class MovieSearchEngine : BaseMediaEngine, IMovieEngine
    {
        public MovieSearchEngine(IPrincipal identity, IRequestServiceMain service, IMovieDbApi movApi, IMapper mapper,
            ILogger<MovieSearchEngine> logger, IRuleEvaluator r, OmbiUserManager um, ICacheService mem, ISettingsService<OmbiSettings> s, IRepository<RequestSubscription> sub)
            : base(identity, service, r, um, mem, s, sub)
        {
            MovieApi = movApi;
            Mapper = mapper;
            Logger = logger;
        }

        protected IMovieDbApi MovieApi { get; }
        protected IMapper Mapper { get; }
        private ILogger<MovieSearchEngine> Logger { get; }

        protected const int MovieLimit = 10;

        /// <summary>
        /// Lookups the imdb information.
        /// </summary>
        /// <param name="theMovieDbId">The movie database identifier.</param>
        /// <returns></returns>
        public async Task<SearchMovieViewModel> LookupImdbInformation(int theMovieDbId, string langCode = null)
        {
            langCode = await DefaultLanguageCode(langCode);
            var movieInfo = await Cache.GetOrAdd(nameof(LookupImdbInformation) + langCode + theMovieDbId,
                async () => await MovieApi.GetMovieInformationWithExtraInfo(theMovieDbId, langCode),
                DateTime.Now.AddHours(12));
            var viewMovie = Mapper.Map<SearchMovieViewModel>(movieInfo);

            return await ProcessSingleMovie(viewMovie, true);
        }



        /// <summary>
        /// Searches the specified movie.
        /// </summary>
        public async Task<IEnumerable<SearchMovieViewModel>> Search(string search, int? year, string langaugeCode)
        {
            langaugeCode = await DefaultLanguageCode(langaugeCode);
            var result = await MovieApi.SearchMovie(search, year, langaugeCode);

            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.Take(ResultLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        public async Task<IEnumerable<SearchMovieViewModel>> SearchActor(string search, string langaugeCode)
        {
            langaugeCode = await DefaultLanguageCode(langaugeCode);
            var people = await MovieApi.SearchByActor(search, langaugeCode);
            var person = people?.results?.Count > 0 ? people.results.FirstOrDefault() : null;

            var resultSet = new List<SearchMovieViewModel>();
            if (person == null)
            {
                return resultSet;
            }
            
            // Get this person movie credits
            var credits = await MovieApi.GetActorMovieCredits(person.id, langaugeCode);
            // Grab results from both cast and crew, prefer items in cast.  we can handle directors like this.
            var movieResults = (from role in credits.cast select new  { Id = role.id, Title = role.title, ReleaseDate = role.release_date }).ToList();
            movieResults.AddRange((from job in credits.crew select new { Id = job.id, Title = job.title, ReleaseDate = job.release_date }).ToList());

            movieResults = movieResults.Take(10).ToList();
            foreach (var movieResult in movieResults)
            {
                resultSet.Add(await LookupImdbInformation(movieResult.Id, langaugeCode));
            }

            return resultSet;
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
                return await TransformMovieResultsToResponse(result.Take(ResultLimit)); // Take x to stop us overloading the API
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
                return await TransformMovieResultsToResponse(result.Take(ResultLimit)); // Take x to stop us overloading the API
            }
            return null;
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
                return await TransformMovieResultsToResponse(result.Take(ResultLimit)); // Take x to stop us overloading the API
            }
            return null;
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
                return await TransformMovieResultsToResponse(result.Take(ResultLimit)); // Take x to stop us overloading the API
            }
            return null;
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
                return await TransformMovieResultsToResponse(result.Take(ResultLimit)); // Take x to stop us overloading the API
            }
            return null;
        }

        protected async Task<List<SearchMovieViewModel>> TransformMovieResultsToResponse(
            IEnumerable<MovieSearchResult> movies)
        {
            var viewMovies = new List<SearchMovieViewModel>();
            foreach (var movie in movies)
            {
                viewMovies.Add(await ProcessSingleMovie(movie));
            }
            return viewMovies;
        }

        protected async Task<SearchMovieViewModel> ProcessSingleMovie(SearchMovieViewModel viewMovie, bool lookupExtraInfo = false)
        {
            if (lookupExtraInfo && viewMovie.ImdbId.IsNullOrEmpty())
            {
                var showInfo = await MovieApi.GetMovieInformation(viewMovie.Id);
                viewMovie.Id = showInfo.Id; // TheMovieDbId
                viewMovie.ImdbId = showInfo.ImdbId;
            }

            var usDates = viewMovie.ReleaseDates?.Results?.FirstOrDefault(x => x.IsoCode == "US");
            viewMovie.DigitalReleaseDate = usDates?.ReleaseDate?.FirstOrDefault(x => x.Type == ReleaseDateType.Digital)?.ReleaseDate;

            viewMovie.TheMovieDbId = viewMovie.Id.ToString();

            await RunSearchRules(viewMovie);

            // This requires the rules to be run first to populate the RequestId property
            await CheckForSubscription(viewMovie);
            
            return viewMovie;
        }

        private async Task CheckForSubscription(SearchMovieViewModel viewModel)
        {
            // Check if this user requested it
            var user = await GetUser();
            if (user == null)
            {
                return;
            }
            var request = await RequestService.MovieRequestService.GetAll()
                .AnyAsync(x => x.RequestedUserId.Equals(user.Id) && x.TheMovieDbId == viewModel.Id);
            if (request || viewModel.Available)
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

        private async Task<SearchMovieViewModel> ProcessSingleMovie(MovieSearchResult movie)
        {
            var viewMovie = Mapper.Map<SearchMovieViewModel>(movie);
            return await ProcessSingleMovie(viewMovie);
        }
    }
}