using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Helpers;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.Api.TheMovieDb
{
    public class TheMovieDbApi : IMovieDbApi
    {
        public TheMovieDbApi(IMapper mapper)
        {
            Api = new Ombi.Api.Api();
            Mapper = mapper;
        }

        private IMapper Mapper { get; }
        private readonly string ApiToken = "b8eabaf5608b88d0298aa189dd90bf00";
        private static readonly string BaseUri ="http://api.themoviedb.org/3/";
        private Ombi.Api.Api Api { get; }

        public async Task<MovieResponseDto> GetMovieInformation(int movieId)
        {
            var request = new Request($"movie/{movieId}", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);

            var result = await Api.Request<MovieResponse>(request);
            return Mapper.Map<MovieResponseDto>(result);
        }

        public async Task<MovieResponseDto> GetMovieInformationWithVideo(int movieId)
        {
            var request = new Request($"movie/{movieId}", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("append_to_response", "videos");
            var result = await Api.Request<MovieResponse>(request);
            return Mapper.Map<MovieResponseDto>(result);
        }

        public async Task<List<MovieSearchResult>> SearchMovie(string searchTerm)
        {
            var request = new Request($"search/movie", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("query", searchTerm);

            var result =  await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<List<MovieSearchResult>> PopularMovies()
        {
            var request = new Request($"movie/popular", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<List<MovieSearchResult>> TopRated()
        {
            var request = new Request($"movie/top_rated", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<List<MovieSearchResult>> Upcoming()
        {
            var request = new Request($"movie/upcoming", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<List<MovieSearchResult>> NowPlaying()
        {
            var request = new Request($"movie/now_playing", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

    }
}
