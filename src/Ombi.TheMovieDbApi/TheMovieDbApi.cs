using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Nito.AsyncEx;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.Api.TheMovieDb
{
    public class TheMovieDbApi : IMovieDbApi
    {
        public TheMovieDbApi(IMapper mapper, IApi api, ISettingsService<TheMovieDbSettings> settingsService)
        {
            Api = api;
            Mapper = mapper;
            Settings = new AsyncLazy<TheMovieDbSettings>(() => settingsService.GetSettingsAsync());
        }

        private const string ApiToken = "b8eabaf5608b88d0298aa189dd90bf00";
        private const string BaseUri ="http://api.themoviedb.org/3/";
        private IMapper Mapper { get; }
        private IApi Api { get; }
        private AsyncLazy<TheMovieDbSettings> Settings { get; }

        public async Task<MovieResponseDto> GetMovieInformation(int movieId)
        {
            var request = new Request($"movie/{movieId}", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            AddRetry(request);

            var result = await Api.Request<MovieResponse>(request);
            return Mapper.Map<MovieResponseDto>(result);
        }

        public async Task<FindResult> Find(string externalId, ExternalSource source)
        {
            var request = new Request($"find/{externalId}", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            AddRetry(request);

            request.AddQueryString("external_source", source.ToString());

            return await Api.Request<FindResult>(request);
        }

        public async Task<TheMovieDbContainer<ActorResult>> SearchByActor(string searchTerm, string langCode)
        {
            var request = new Request($"search/person", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("query", searchTerm);
            request.AddQueryString("language", langCode);
            var settings = await Settings;
            request.AddQueryString("include_adult", settings.ShowAdultMovies.ToString().ToLower());

            var result = await Api.Request<TheMovieDbContainer<ActorResult>>(request);
            return result;
        }

        public async Task<ActorCredits> GetActorMovieCredits(int actorId, string langCode)
        {
            var request = new Request($"person/{actorId}/movie_credits", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);

            var result = await Api.Request<ActorCredits>(request);
            return result;
        }

        public async Task<List<TvSearchResult>> SearchTv(string searchTerm)
        {
            var request = new Request($"search/tv", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("query", searchTerm);
            AddRetry(request);

            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<TvSearchResult>>(result.results);
        }

        public async Task<TvExternals> GetTvExternals(int theMovieDbId)
        {
            var request = new Request($"/tv/{theMovieDbId}/external_ids", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            AddRetry(request);

            return await Api.Request<TvExternals>(request);
        }
        
        public async Task<List<MovieSearchResult>> SimilarMovies(int movieId, string langCode)
        {
            var request = new Request($"movie/{movieId}/similar", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);
            AddRetry(request);

            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<MovieResponseDto> GetMovieInformationWithExtraInfo(int movieId, string langCode = "en")
        {
            var request = new Request($"movie/{movieId}", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("append_to_response", "videos,release_dates");
            request.AddQueryString("language", langCode);
            AddRetry(request);
            var result = await Api.Request<MovieResponse>(request);
            return Mapper.Map<MovieResponseDto>(result);
        }

        public async Task<List<MovieSearchResult>> SearchMovie(string searchTerm, int? year, string langCode)
        {
            var request = new Request($"search/movie", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("query", searchTerm);
            request.AddQueryString("language", langCode);
            if (year.HasValue && year.Value > 0)
            {
                request.AddQueryString("year", year.Value.ToString());
            }

            var settings = await Settings;
            request.AddQueryString("include_adult", settings.ShowAdultMovies.ToString().ToLower());

            AddRetry(request);

            var result =  await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<List<MovieSearchResult>> PopularMovies(string langCode)
        {
            var request = new Request($"discover/movie", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);
            var settings = await Settings;
            request.AddQueryString("include_adult", settings.ShowAdultMovies.ToString().ToLower());
            request.AddQueryString("without_keywords", settings.ExcludedKeywordIds);

            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<List<MovieSearchResult>> TopRated(string langCode)
        {
            var request = new Request($"discover/movie", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);
            request.AddQueryString("vote_count.gte", "250");
            request.AddQueryString("sort_by", "vote_average.desc");
            var settings = await Settings;
            request.AddQueryString("include_adult", settings.ShowAdultMovies.ToString().ToLower());
            request.AddQueryString("without_keywords", settings.ExcludedKeywordIds);

            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<List<MovieSearchResult>> Upcoming(string langCode)
        {
            var request = new Request($"discover/movie", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);
            request.AddQueryString("with_release_type", "2|3");
            var startDate = DateTime.Today.AddDays(7);
            request.AddQueryString("release_date.gte", startDate.ToString("yyyy-MM-dd"));
            request.AddQueryString("release_date.lte", startDate.AddDays(17).ToString("yyyy-MM-dd"));
            var settings = await Settings;
            request.AddQueryString("include_adult", settings.ShowAdultMovies.ToString().ToLower());
            request.AddQueryString("without_keywords", settings.ExcludedKeywordIds);

            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<List<MovieSearchResult>> NowPlaying(string langCode)
        {
            var request = new Request($"discover/movie", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);
            request.AddQueryString("with_release_type", "2|3");
            var today = DateTime.Today;
            request.AddQueryString("release_date.gte", today.AddDays(-42).ToString("yyyy-MM-dd"));
            request.AddQueryString("release_date.lte", today.AddDays(6).ToString("yyyy-MM-dd"));
            var settings = await Settings;
            request.AddQueryString("include_adult", settings.ShowAdultMovies.ToString().ToLower());
            request.AddQueryString("without_keywords", settings.ExcludedKeywordIds);

            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<TvInfo> GetTVInfo(string themoviedbid)
        {
            var request = new Request($"/tv/{themoviedbid}", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            AddRetry(request);

            return await Api.Request<TvInfo>(request);
        }
        private static void AddRetry(Request request)
        {
            request.Retry = true;
            request.StatusCodeToRetry.Add((HttpStatusCode)429);
        }
    }
}
