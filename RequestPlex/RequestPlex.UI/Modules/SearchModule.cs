using Nancy;
using Nancy.Responses.Negotiation;

using RequestPlex.Api;

namespace RequestPlex.UI.Modules
{
    public class SearchModule : NancyModule
    {
        public SearchModule()
        {
            Get["search/"] = parameters => RequestLoad();

            Get["search/movie/{searchTerm}"] = parameters =>
            {
                var search = (string)parameters.searchTerm;
                return SearchMovie(search);
            };

            Get["search/tv/{searchTerm}"] = parameters =>
            {
                var search = (string)parameters.searchTerm;
                return SearchTvShow(search);
            };

            Get["search/movie/upcoming"] = parameters => UpcomingMovies();
            Get["search/movie/playing"] = parameters => CurrentlyPlayingMovies();

            Post["search/request/movie"] = parameters =>
            {
                var movieId = (int)Request.Form.movieId;
                return RequestMovie(movieId);
            };

            Post["search/request/tv"] = parameters =>
            {
                var tvShowId = (int)Request.Form.showId;
                var latest = (bool)Request.Form.latestSeason;
                return RequestTvShow(tvShowId, latest);
            };
        }

        private Negotiator RequestLoad()
        {
            return View["Search/Index"];
        }

        private Response SearchMovie(string searchTerm)
        {
            var api = new TheMovieDbApi();
            var movies = api.SearchMovie(searchTerm);
            var result = movies.Result;
            return Response.AsJson(result);
        }

        private Response SearchTvShow(string searchTerm)
        {
            var api = new TheMovieDbApi();
            var tvShow = api.SearchTv(searchTerm);
            var result = tvShow.Result;
            return Response.AsJson(result);
        }

        private Response UpcomingMovies()
        {
            var api = new TheMovieDbApi();
            var movies = api.GetUpcomingMovies();
            var result = movies.Result;
            return Response.AsJson(result);
        }

        private Response CurrentlyPlayingMovies()
        {
            var api = new TheMovieDbApi();
            var movies = api.GetCurrentPlayingMovies();
            var result = movies.Result;
            return Response.AsJson(result);
        }

        private Response RequestMovie(int movieId)
        {
            return Response.AsJson("");
        }

        private Response RequestTvShow(int showId, bool latest)
        {
            return Response.AsJson("");
        }
    }
}