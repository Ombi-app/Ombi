using Nancy;
using Nancy.Responses.Negotiation;

using RequestPlex.Api;
using RequestPlex.Api.Models.Tv;
using RequestPlex.Core;
using RequestPlex.Helpers;
using RequestPlex.Store;

namespace RequestPlex.UI.Modules
{
    public class SearchModule : NancyModule
    {
        public SearchModule(ICacheProvider cache) : base("search")
        {
            MovieApi = new TheMovieDbApi();
            TvApi = new TheTvDbApi();
            Cache = cache;
            
            Get["/"] = parameters => RequestLoad();

            Get["movie/{searchTerm}"] = parameters => SearchMovie((string)parameters.searchTerm);
            Get["tv/{searchTerm}"] = parameters => SearchTvShow((string)parameters.searchTerm);

            Get["movie/upcoming"] = parameters => UpcomingMovies();
            Get["movie/playing"] = parameters => CurrentlyPlayingMovies();

            Post["request/movie"] = parameters => RequestMovie((int)Request.Form.movieId);
            Post["request/tv"] = parameters => RequestTvShow((int)Request.Form.tvId, (bool)Request.Form.latest);
        }
        private TheMovieDbApi MovieApi { get; }
        private TheTvDbApi TvApi { get; }
        private ICacheProvider Cache { get; }
        private string AuthToken => Cache.GetOrSet(CacheKeys.TvDbToken, TvApi.Authenticate, 50);

        private Negotiator RequestLoad()
        { 
            return View["Search/Index"];
        }

        private Response SearchMovie(string searchTerm)
        {
            var movies = MovieApi.SearchMovie(searchTerm);
            var result = movies.Result;
            return Response.AsJson(result);
        }

        private Response SearchTvShow(string searchTerm)
        {
            var tvShow = TvApi.SearchTv(searchTerm, AuthToken);
            return Response.AsJson(tvShow);
        }

        private Response UpcomingMovies()
        {
            var movies = MovieApi.GetUpcomingMovies();
            var result = movies.Result;
            return Response.AsJson(result);
        }

        private Response CurrentlyPlayingMovies()
        {
            var movies = MovieApi.GetCurrentPlayingMovies();
            var result = movies.Result;
            return Response.AsJson(result);
        }

        private Response RequestMovie(int movieId)
        {
            var s = new SettingsService();
            if (s.CheckRequest(movieId))
            {
                return Response.AsJson(new { Result = false, Message = "Movie has already been requested!" });
            }
            
            s.AddRequest(movieId, RequestType.Movie);
            return Response.AsJson(new { Result = true });
        }

        /// <summary>
        /// Requests the tv show.
        /// </summary>
        /// <param name="showId">The show identifier.</param>
        /// <param name="latest">if set to <c>true</c> [latest].</param>
        /// <returns></returns>
        private Response RequestTvShow(int showId, bool latest)
        {
            // Latest send to Sonarr and no need to store in DB
            var s = new SettingsService();
            if (s.CheckRequest(showId))
            {
                return Response.AsJson(new { Result = false, Message = "TV Show has already been requested!" });
            }
            s.AddRequest(showId, RequestType.TvShow);
            return Response.AsJson(new {Result = true });
        }
    }
}