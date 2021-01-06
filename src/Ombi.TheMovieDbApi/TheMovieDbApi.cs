using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Nito.AsyncEx;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
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
        private const string BaseUri = "http://api.themoviedb.org/3/";
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


        public async Task<FullMovieInfo> GetFullMovieInfo(int movieId, CancellationToken cancellationToken, string langCode)
        {
            var request = new Request($"movie/{movieId}", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("language", langCode);
            request.FullUri = request.FullUri.AddQueryParameter("append_to_response", "videos,credits,similar,recommendations,release_dates,external_ids,keywords");
            AddRetry(request);

            return await Api.Request<FullMovieInfo>(request, cancellationToken);
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

        public async Task<Collections> GetCollection(string langCode, int collectionId, CancellationToken cancellationToken)
        {
            // https://developers.themoviedb.org/3/discover/movie-discover
            var request = new Request($"collection/{collectionId}", BaseUri, HttpMethod.Get);
            request.FullUri = request.FullUri.AddQueryParameter("api_key", ApiToken);
            request.FullUri = request.FullUri.AddQueryParameter("language", langCode);

            return await Api.Request<Collections>(request, cancellationToken);
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

        public async Task<List<TvSearchResult>> SearchTv(string searchTerm, string year = default)
        {
            var request = new Request($"search/tv", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("query", searchTerm);
            if (year.HasValue())
            {
                request.AddQueryString("first_air_date_year", year);
            }
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

            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        /// <remarks>
        /// Maintains filter parity with <a href="https://developers.themoviedb.org/3/movies/get-popular-movies">/movie/popular</a>.
        /// </remarks>
        public async Task<List<MovieSearchResult>> PopularMovies(string langCode, int? page = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new Request($"discover/movie", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);
            request.AddQueryString("sort_by", "popularity.desc");
            if (page != null)
            {
                request.AddQueryString("page", page.ToString());
            }
            await AddDiscoverMovieSettings(request);
            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request, cancellationToken);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        /// <remarks>
        /// Maintains filter parity with <a href="https://developers.themoviedb.org/3/movies/get-top-rated-movies">/movie/top_rated</a>.
        /// </remarks>
        public async Task<List<MovieSearchResult>> TopRated(string langCode, int? page = null)
        {
            var request = new Request($"discover/movie", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);
            request.AddQueryString("sort_by", "vote_average.desc");
            if (page != null)
            {
                request.AddQueryString("page", page.ToString());
            }

            // `vote_count` consideration isn't explicitly documented, but using only the `sort_by` filter
            // does not provide the same results as `/movie/top_rated`. This appears to be adequate enough
            // to filter out extremely high-rated movies due to very little votes
            request.AddQueryString("vote_count.gte", "250");

            await AddDiscoverMovieSettings(request);
            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        /// <remarks>
        /// Maintains filter parity with <a href="https://developers.themoviedb.org/3/movies/get-upcoming">/movie/upcoming</a>.
        /// </remarks>
        public async Task<List<MovieSearchResult>> Upcoming(string langCode, int? page = null)
        {
            var request = new Request($"discover/movie", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);

            // Release types "2 or 3" explicitly stated as used in docs
            request.AddQueryString("with_release_type", "2|3");

            // The date range being used in `/movie/upcoming` isn't documented, but we infer it is
            // an offset from today based on the minimum and maximum date they provide in the output
            var startDate = DateTime.Today.AddDays(7);
            request.AddQueryString("release_date.gte", startDate.ToString("yyyy-MM-dd"));
            request.AddQueryString("release_date.lte", startDate.AddDays(17).ToString("yyyy-MM-dd"));
            if (page != null)
            {
                request.AddQueryString("page", page.ToString());
            }
            await AddDiscoverMovieSettings(request);
            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        /// <remarks>
        /// Maintains filter parity with <a href="https://developers.themoviedb.org/3/movies/get-now-playing">/movie/now_playing</a>.
        /// </remarks>
        public async Task<List<MovieSearchResult>> NowPlaying(string langCode, int? page = null)
        {
            var request = new Request($"discover/movie", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);

            // Release types "2 or 3" explicitly stated as used in docs
            request.AddQueryString("with_release_type", "2|3");

            // The date range being used in `/movie/now_playing` isn't documented, but we infer it is
            // an offset from today based on the minimum and maximum date they provide in the output
            var today = DateTime.Today;
            request.AddQueryString("release_date.gte", today.AddDays(-42).ToString("yyyy-MM-dd"));
            request.AddQueryString("release_date.lte", today.AddDays(6).ToString("yyyy-MM-dd"));
            if (page != null)
            {
                request.AddQueryString("page", page.ToString());
            }

            await AddDiscoverMovieSettings(request);
            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieSearchResult>>(result.results);
        }

        public async Task<TvInfo> GetTVInfo(string themoviedbid)
        {
            var request = new Request($"/tv/{themoviedbid}", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("append_to_response", "external_ids");
            AddRetry(request);

            return await Api.Request<TvInfo>(request);
        }

        public async Task<List<Keyword>> SearchKeyword(string searchTerm)
        {
            var request = new Request("search/keyword", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("query", searchTerm);
            AddRetry(request);

            var result = await Api.Request<TheMovieDbContainer<Keyword>>(request);
            return result.results ?? new List<Keyword>();
        }

        public async Task<Keyword> GetKeyword(int keywordId)
        {
            var request = new Request($"keyword/{keywordId}", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            AddRetry(request);

            var keyword = await Api.Request<Keyword>(request);
            return keyword == null || keyword.Id == 0 ? null : keyword;
        }

        public Task<TheMovieDbContainer<MultiSearch>> MultiSearch(string searchTerm, string languageCode, CancellationToken cancellationToken)
        {
            var request = new Request("search/multi", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", languageCode);
            request.AddQueryString("query", searchTerm);
            var result = Api.Request<TheMovieDbContainer<MultiSearch>>(request, cancellationToken);
            return result;
        }

        public Task<WatchProviders> GetMovieWatchProviders(int theMoviedbId, CancellationToken token)
        {
            var request = new Request($"movie/{theMoviedbId}/watch/providers", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);

            return Api.Request<WatchProviders>(request, token);
        }

        public Task<WatchProviders> GetTvWatchProviders(int theMoviedbId, CancellationToken token)
        {
            var request = new Request($"tv/{theMoviedbId}/watch/providers", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);

            return Api.Request<WatchProviders>(request, token);
        }

        private async Task AddDiscoverMovieSettings(Request request)
        {
            var settings = await Settings;
            request.AddQueryString("include_adult", settings.ShowAdultMovies.ToString().ToLower());
            if (settings.ExcludedKeywordIds?.Any() == true)
            {
                request.AddQueryString("without_keywords", string.Join(",", settings.ExcludedKeywordIds));
            }
        }

        private static void AddRetry(Request request)
        {
            request.Retry = true;
            request.StatusCodeToRetry.Add((HttpStatusCode)429);
        }
    }
}
