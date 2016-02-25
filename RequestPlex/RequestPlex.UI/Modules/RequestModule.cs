using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;

using RequestPlex.Api;

namespace RequestPlex.UI.Modules
{
    public class RequestModule : NancyModule
    {
        public RequestModule()
        {
            Get["request/"] = parameters => RequestLoad();

            Get["request/movie/{searchTerm}"] = parameters =>
            {
                var search = (string)parameters.searchTerm;
                return SearchMovie(search);
            };

            Get["request/tv/{searchTerm}"] = parameters =>
            {
                var search = (string)parameters.searchTerm;
                return SearchTvShow(search);
            };
        }

        private Negotiator RequestLoad()
        {
            return View["Request/Index"];
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
    }
}