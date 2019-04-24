﻿using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Ombi.Api.TheMovieDb.Models;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.Api.TheMovieDb
{
    public class TheMovieDbApi : IMovieDbApi
    {
        public TheMovieDbApi(IMapper mapper, IApi api)
        {
            Api = api;
            Mapper = mapper;
        }

        private IMapper Mapper { get; }
        private readonly string ApiToken = "b8eabaf5608b88d0298aa189dd90bf00";
        private static readonly string BaseUri ="http://api.themoviedb.org/3/";
        private IApi Api { get; }

        public async Task<MovieResponseDto> GetMovieInformation(int movieId)
        {
            var request = new Request($"movie/{movieId}", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            AddRetry(request);

            var result = await Api.Request<MovieResponse>(request);
            return Mapper.Map<MovieResponseDto>(result);
        }


        public async Task<FullMovieInfo> GetFullMovieInfo(int movieId, string langCode)
        {
            var request = new Request($"movie/{movieId}", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("language", langCode);
            request.FullUri = request.FullUri.AddQueryParameter("append_to_response", "videos,credits,similar,recommendations,release_dates,external_ids,keywords");
            AddRetry(request);

            return await Api.Request<FullMovieInfo>(request);
        }

        public async Task<TheMovieDbContainer<DiscoverMovies>> DiscoverMovies(string langCode, int keywordId)
        {
            // https://developers.themoviedb.org/3/discover/movie-discover
            var request = new Request("discover/movie", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("language", langCode);
            request.FullUri = request.FullUri.AddQueryParameter("with_keyword", keywordId.ToString());
            request.FullUri = request.FullUri.AddQueryParameter("sort_by", "popularity.desc");

            return await Api.Request<TheMovieDbContainer<DiscoverMovies>>(request);
        }

        public async Task<Collections> GetCollection(string langCode, int collectionId)
        {
            // https://developers.themoviedb.org/3/discover/movie-discover
            var request = new Request($"collection/{collectionId}", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("language", langCode);

            return await Api.Request<Collections> (request);
        }

        public async Task<FindResult> Find(string externalId, ExternalSource source)
        {
            var request = new Request($"find/{externalId}", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            AddRetry(request);

            request.AddQueryString("external_source", source.ToString());

            return await Api.Request<FindResult>(request);
        }

        public async Task<TheMovieDbContainer<ActorResult>> SearchByActor(string searchTerm, string langCode)
        {
            var request = new Request($"search/person", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("query", searchTerm);
            request.FullUri = request.FullUri.AddQueryParameter("language", langCode);

            var result = await Api.Request<TheMovieDbContainer<ActorResult>>(request);
            return result;
        }

        public async Task<ActorCredits> GetActorMovieCredits(int actorId, string langCode)
        {
            var request = new Request($"person/{actorId}/movie_credits", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("language", langCode);

            var result = await Api.Request<ActorCredits>(request);
            return result;
        }

        public async Task<List<TvSearchResult>> SearchTv(string searchTerm)
        {
            var request = new Request($"search/tv", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("query", searchTerm);
            AddRetry(request);

            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<TvSearchResult>>(result.results);
        }

        public async Task<TvExternals> GetTvExternals(int theMovieDbId)
        {
            var request = new Request($"/tv/{theMovieDbId}/external_ids", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            AddRetry(request);

            return await Api.Request<TvExternals>(request);
        }
        
        public async Task<List<MovieSearchResult>> SimilarMovies(int movieId, string langCode)
        {
            var request = new Request($"movie/{movieId}/similar", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);

            request.FullUri = request.FullUri.AddQueryParameter("language", langCode);
            AddRetry(request);

            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<MovieResponseDto> GetMovieInformationWithExtraInfo(int movieId, string langCode = "en")
        {
            var request = new Request($"movie/{movieId}", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("append_to_response", "videos,release_dates");
            request.FullUri = request.FullUri.AddQueryParameter("language", langCode);
            AddRetry(request);
            var result = await Api.Request<MovieResponse>(request);
            return Mapper.Map<MovieResponseDto>(result);
        }

        public async Task<List<MovieSearchResult>> SearchMovie(string searchTerm, int? year, string langageCode)
        {
            var request = new Request($"search/movie", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("query", searchTerm);
            request.FullUri = request.FullUri.AddQueryParameter("language", langageCode);
            if (year.HasValue && year.Value > 0)
            {
                request.FullUri = request.FullUri.AddQueryParameter("year", year.Value.ToString());
            }
            AddRetry(request);

            var result =  await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<List<MovieSearchResult>> PopularMovies(string langageCode, int? page = null)
        {
            var request = new Request($"movie/popular", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("language", langageCode);
            if (page != null)
            {
                request.FullUri = request.FullUri.AddQueryParameter("page", page.ToString());
            }
            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<List<MovieSearchResult>> TopRated(string langageCode, int? page = null)
        {
            var request = new Request($"movie/top_rated", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("language", langageCode);
            if (page != null)
            {
                request.FullUri = request.FullUri.AddQueryParameter("page", page.ToString());
            }
            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<List<MovieSearchResult>> Upcoming(string langageCode, int? page = null)
        {
            var request = new Request($"movie/upcoming", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("language", langageCode);
            if (page != null)
            {
                request.FullUri = request.FullUri.AddQueryParameter("page", page.ToString());
            }
            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<List<MovieSearchResult>> NowPlaying(string langageCode, int? page = null)
        {
            var request = new Request($"movie/now_playing", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("language", langageCode);
            if (page != null)
            {
                request.FullUri = request.FullUri.AddQueryParameter("page", page.ToString());
            }
            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<TvInfo> GetTVInfo(string themoviedbid)
        {
            var request = new Request($"/tv/{themoviedbid}", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("append_to_response", "external_ids");
            AddRetry(request);

            return await Api.Request<TvInfo>(request);
        }

        public async Task<TheMovieDbContainer<MultiSearch>> MultiSearch(string searchTerm, string languageCode)
        {
            var request = new Request("search/multi", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("language", languageCode);
            request.FullUri = request.FullUri.AddQueryParameter("query", searchTerm);
            var result = await Api.Request<TheMovieDbContainer<MultiSearch>>(request);
            return result;
        }

        private static void AddRetry(Request request)
        {
            request.Retry = true;
            request.StatusCodeToRetry.Add((HttpStatusCode)429);
        }
    }
}
