using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Requests.Models;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.TheMovieDbApi;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.Core.Engine
{
    public class MovieEngine : IMovieEngine
    {

        public MovieEngine(IRequestService service, IMovieDbApi movApi)
        {
            RequestService = service;
            MovieApi = movApi;
        }
        private IRequestService RequestService { get; }
        private IMovieDbApi MovieApi { get; }

        public async Task<IEnumerable<SearchMovieViewModel>> LookupImdbInformation(IEnumerable<SearchMovieViewModel> movies)
        {
            var retVal = new List<SearchMovieViewModel>();
            Dictionary<int, RequestModel> dbMovies = await RequestedMovies();
            foreach (var m in movies)
            {

                var movieInfo = await MovieApi.GetMovieInformationWithVideo(m.Id);
                var viewMovie = new SearchMovieViewModel
                {
                    Adult = movieInfo.adult,
                    BackdropPath = movieInfo.backdrop_path,
                    Id = movieInfo.id,
                    OriginalLanguage = movieInfo.original_language,
                    OriginalTitle = movieInfo.original_title,
                    Overview = movieInfo.overview,
                    Popularity = movieInfo.popularity,
                    PosterPath = movieInfo.poster_path,
                    ReleaseDate =
                        string.IsNullOrEmpty(movieInfo.release_date)
                            ? DateTime.MinValue
                            : DateTime.Parse(movieInfo.release_date),
                    Title = movieInfo.title,
                    Video = movieInfo.video,
                    VoteAverage = movieInfo.vote_average,
                    VoteCount = movieInfo.vote_count,
                    ImdbId = movieInfo?.imdb_id,
                    Homepage = movieInfo?.homepage
                };
                retVal.Add(viewMovie);
                // TODO needs to be careful about this, it's adding extra time to search...
                // https://www.themoviedb.org/talk/5807f4cdc3a36812160041f2
                //var videoId = movieInfo?.video ?? false
                //    ? movieInfo?.videos?.results?.FirstOrDefault()?.key
                //    : string.Empty;

                //viewMovie.Trailer = string.IsNullOrEmpty(videoId)
                //    ? string.Empty
                //    : $"https://www.youtube.com/watch?v={videoId}";
                if (dbMovies.ContainsKey(movieInfo.id) /*&& canSee*/) // compare to the requests db
                {
                    var dbm = dbMovies[movieInfo.id];

                    viewMovie.Requested = true;
                    viewMovie.Approved = dbm.Approved;
                    viewMovie.Available = dbm.Available;
                }
            }
            return retVal;
        }

        public async Task<IEnumerable<SearchMovieViewModel>> ProcessMovieSearch(string search)
        {
            var api = new TheMovieDbApi.TheMovieDbApi();
            var result = await api.SearchMovie(search);
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.results);
            }
            return null;
        }
        public async Task<IEnumerable<SearchMovieViewModel>> PopularMovies()
        {
            var api = new TheMovieDbApi.TheMovieDbApi();
            var result = await api.PopularMovies();
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.results);
            }
            return null;
        }

        public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies()
        {
            var api = new TheMovieDbApi.TheMovieDbApi();
            var result = await api.TopRated();
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.results);
            }
            return null;
        }

        public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies()
        {
            var api = new TheMovieDbApi.TheMovieDbApi();
            var result = await api.Upcoming();
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.results);
            }
            return null;
        }
        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies()
        {
            var api = new TheMovieDbApi.TheMovieDbApi();
            var result = await api.NowPlaying();
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result.results);
            }
            return null;
        }


        private async Task<List<SearchMovieViewModel>> TransformMovieResultsToResponse(IEnumerable<SearchResult> movies)
        {
            await Task.Yield();
            var viewMovies = new List<SearchMovieViewModel>();
            //var counter = 0;
            Dictionary<int, RequestModel> dbMovies = await RequestedMovies();
            foreach (var movie in movies)
            {
                var viewMovie = new SearchMovieViewModel
                {
                    Adult = movie.adult,
                    BackdropPath = movie.backdrop_path,
                    Id = movie.id,
                    OriginalLanguage = movie.original_language,
                    OriginalTitle = movie.original_title,
                    Overview = movie.overview,
                    Popularity = movie.popularity,
                    PosterPath = movie.poster_path,
                    ReleaseDate = string.IsNullOrEmpty(movie.release_date) ? DateTime.MinValue : DateTime.Parse(movie.release_date),
                    Title = movie.title,
                    Video = movie.video,
                    VoteAverage = movie.vote_average,
                    VoteCount = movie.vote_count
                };
                viewMovies.Add(viewMovie);



                //    var canSee = CanUserSeeThisRequest(viewMovie.Id, Security.HasPermissions(User, Permissions.UsersCanViewOnlyOwnRequests), dbMovies);

                //    var plexSettings = await PlexService.GetSettingsAsync();
                //    var embySettings = await EmbySettings.GetSettingsAsync();
                //    if (plexSettings.Enable)
                //    {
                //        var content = PlexContentRepository.GetAll();
                //        var plexMovies = PlexChecker.GetPlexMovies(content);

                //        var plexMovie = PlexChecker.GetMovie(plexMovies.ToArray(), movie.Title,
                //            movie.ReleaseDate?.Year.ToString(),
                //            viewMovie.ImdbId);
                //        if (plexMovie != null)
                //        {
                //            viewMovie.Available = true;
                //            viewMovie.PlexUrl = plexMovie.Url;
                //        }
                //    }
                //    if (embySettings.Enable)
                //    {
                //        var embyContent = EmbyContentRepository.GetAll();
                //        var embyMovies = EmbyChecker.GetEmbyMovies(embyContent);

                //        var embyMovie = EmbyChecker.GetMovie(embyMovies.ToArray(), movie.Title,
                //            movie.ReleaseDate?.Year.ToString(), viewMovie.ImdbId);
                //        if (embyMovie != null)
                //        {
                //            viewMovie.Available = true;
                //        }
                //    }
                if (dbMovies.ContainsKey(movie.id) /*&& canSee*/) // compare to the requests db
                {
                    var dbm = dbMovies[movie.id];

                    viewMovie.Requested = true;
                    viewMovie.Approved = dbm.Approved;
                    viewMovie.Available = dbm.Available;
                }
                //    else if (canSee)
                //    {
                //        bool exists = IsMovieInCache(movie, viewMovie.ImdbId);
                //        viewMovie.Approved = exists;
                //        viewMovie.Requested = exists;
                //    }
                //    viewMovies.Add(viewMovie);
                //}


            }
            return viewMovies;
        }


        private long _dbMovieCacheTime = 0;
        private Dictionary<int, RequestModel> _dbMovies;
        private async Task<Dictionary<int, RequestModel>> RequestedMovies()
        {
            long now = DateTime.Now.Ticks;
            if (_dbMovies == null || (now - _dbMovieCacheTime) > 10000)
            {
                var allResults = await RequestService.GetAllAsync();
                allResults = allResults.Where(x => x.Type == RequestType.Movie);

                var distinctResults = allResults.DistinctBy(x => x.ProviderId);
                _dbMovies = distinctResults.ToDictionary(x => x.ProviderId);
                _dbMovieCacheTime = now;
            }
            return _dbMovies;
        }
    }
}
