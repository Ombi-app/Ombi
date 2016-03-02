using Nancy;
using Nancy.Responses.Negotiation;

using RequestPlex.Api;
using RequestPlex.Core;
using RequestPlex.Store;

namespace RequestPlex.UI.Modules
{
    public class SearchModule : NancyModule
    {
        public SearchModule() : base("search")
        {
            Api = new TheMovieDbApi();
            Get["/"] = parameters => RequestLoad();

            Get["movie/{searchTerm}"] = parameters => SearchMovie((string)parameters.searchTerm);
            Get["tv/{searchTerm}"] = parameters => SearchTvShow((string)parameters.searchTerm);

            Get["movie/upcoming"] = parameters => UpcomingMovies();
            Get["movie/playing"] = parameters => CurrentlyPlayingMovies();

            Post["request/movie"] = parameters => RequestMovie((int)Request.Form.movieId);
            Post["request/tv"] = parameters => RequestTvShow((int)Request.Form.tvId, (bool)Request.Form.latest);
        }
        private TheMovieDbApi Api { get; }

        private Negotiator RequestLoad()
        {
            return View["Search/Index"];
        }

        private Response SearchMovie(string searchTerm)
        {
            var movies = Api.SearchMovie(searchTerm);
            var result = movies.Result;
            return Response.AsJson(result);
        }

        private Response SearchTvShow(string searchTerm)
        {
            var tvShow = Api.SearchTv(searchTerm);
            var result = tvShow.Result;
            return Response.AsJson(result);
        }

        private Response UpcomingMovies()
        {
            var movies = Api.GetUpcomingMovies();
            var result = movies.Result;
            return Response.AsJson(result);
        }

        private Response CurrentlyPlayingMovies()
        {
            var movies = Api.GetCurrentPlayingMovies();
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