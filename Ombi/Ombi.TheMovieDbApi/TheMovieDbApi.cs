using System.Net.Http;
using System.Threading.Tasks;
using Ombi.Api;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.TheMovieDbApi
{
    public class TheMovieDbApi : IMovieDbApi
    {
        public TheMovieDbApi()
        {
            Api = new Api.Api();
        }
        private const string ApiToken = "b8eabaf5608b88d0298aa189dd90bf00";
        private static readonly string BaseUri ="http://api.themoviedb.org/3/";
        private Api.Api Api { get; }

        public async Task<MovieResponse> GetMovieInformation(int movieId)
        {
            var request = new Request($"movie/{movieId}", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            
            return await Api.Request<MovieResponse>(request);
        }

        public async Task<MovieResponse> GetMovieInformationWithVideo(int movieId)
        {
            var request = new Request($"movie/{movieId}", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("append_to_response", "videos");
            return await Api.Request<MovieResponse>(request);
        }

        public async Task<TheMovieDbContainer<SearchResult>> SearchMovie(string searchTerm)
        {
            var request = new Request($"search/movie", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("query", searchTerm);

            return await Api.Request<TheMovieDbContainer<SearchResult>>(request);
        }

        public async Task<TheMovieDbContainer<SearchResult>> PopularMovies()
        {
            var request = new Request($"movie/popular", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
           
            return await Api.Request<TheMovieDbContainer<SearchResult>>(request);
        }

        public async Task<TheMovieDbContainer<SearchResult>> TopRated()
        {
            var request = new Request($"movie/top_rated", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            return await Api.Request<TheMovieDbContainer<SearchResult>>(request);
        }

        public async Task<TheMovieDbContainer<SearchResult>> Upcoming()
        {
            var request = new Request($"movie/upcoming", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            return await Api.Request<TheMovieDbContainer<SearchResult>>(request);
        }

        public async Task<TheMovieDbContainer<SearchResult>> NowPlaying()
        {
            var request = new Request($"movie/now_playing", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            return await Api.Request<TheMovieDbContainer<SearchResult>>(request);
        }

    }
}
