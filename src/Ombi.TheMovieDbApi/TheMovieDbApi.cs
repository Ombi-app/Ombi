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

// Due to conflicting Genre models in
// Ombi.TheMovieDbApi.Models and Ombi.Api.TheMovieDb.Models   
using Genre = Ombi.TheMovieDbApi.Models.Genre;

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

        public async Task<List<MovieDbSearchResult>> SimilarMovies(int movieId, string langCode)
        {
            var request = new Request($"movie/{movieId}/similar", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);
            AddRetry(request);

            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieDbSearchResult>>(result.results);
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

        public async Task<List<MovieDbSearchResult>> SearchMovie(string searchTerm, int? year, string langCode)
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
            return Mapper.Map<List<MovieDbSearchResult>>(result.results);
        }

        /// <remarks>
        /// Maintains filter parity with <a href="https://developers.themoviedb.org/3/movies/get-popular-movies">/movie/popular</a>.
        /// </remarks>
        public async Task<List<MovieDbSearchResult>> PopularMovies(string langCode, int? page = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Popular("movie", langCode, page, cancellationToken);
        }

        public async Task<List<MovieDbSearchResult>> PopularTv(string langCode, int? page = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Popular("tv", langCode, page, cancellationToken);
        }

        public async Task<List<MovieDbSearchResult>> Popular(string type, string langCode, int? page = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new Request($"discover/{type}", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);
            request.AddQueryString("sort_by", "popularity.desc");
            if (page != null)
            {
                request.AddQueryString("page", page.ToString());
            }
            await AddDiscoverSettings(request);
            await AddGenreFilter(request, type);
            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request, cancellationToken);
            return Mapper.Map<List<MovieDbSearchResult>>(result.results);
        }

        public Task<List<MovieDbSearchResult>> TopRated(string langCode, int? page = null)
        {
            return TopRated("movie", langCode, page);
        }

        public Task<List<MovieDbSearchResult>> TopRatedTv(string langCode, int? page = null)
        {
            return TopRated("tv", langCode, page);
        }

        /// <remarks>
        /// Maintains filter parity with <a href="https://developers.themoviedb.org/3/movies/get-top-rated-movies">/movie/top_rated</a>.
        /// </remarks>
        private async Task<List<MovieDbSearchResult>> TopRated(string type, string langCode, int? page = null)
        {
            var request = new Request($"discover/{type}", BaseUri, HttpMethod.Get);
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

            await AddDiscoverSettings(request);
            await AddGenreFilter(request, type);
            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieDbSearchResult>>(result.results);
        }

        public Task<List<MovieDbSearchResult>> Upcoming(string langCode, int? page = null)
        {
            return Upcoming("movie", langCode, page);
        }
        public Task<List<MovieDbSearchResult>> UpcomingTv(string langCode, int? page = null)
        {
            return Upcoming("tv", langCode, page);
        }

        /// <remarks>
        /// Maintains filter parity with <a href="https://developers.themoviedb.org/3/movies/get-upcoming">/movie/upcoming</a>.
        /// </remarks>
        private async Task<List<MovieDbSearchResult>> Upcoming(string type, string langCode, int? page = null)
        {
            var request = new Request($"discover/{type}", BaseUri, HttpMethod.Get);
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
            await AddDiscoverSettings(request);
            await AddGenreFilter(request, type);
            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieDbSearchResult>>(result.results);
        }

        /// <remarks>
        /// Maintains filter parity with <a href="https://developers.themoviedb.org/3/movies/get-now-playing">/movie/now_playing</a>.
        /// </remarks>
        public async Task<List<MovieDbSearchResult>> NowPlaying(string langCode, int? page = null)
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

            await AddDiscoverSettings(request);
            await AddGenreFilter(request, "movie");
            AddRetry(request);
            var result = await Api.Request<TheMovieDbContainer<SearchResult>>(request);
            return Mapper.Map<List<MovieDbSearchResult>>(result.results);
        }

        public async Task<TvInfo> GetTVInfo(string themoviedbid, string langCode = "en")
        {
            var request = new Request($"/tv/{themoviedbid}", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);
            request.AddQueryString("append_to_response", "videos,credits,similar,recommendations,external_ids,keywords,images");
            AddRetry(request);

            return await Api.Request<TvInfo>(request);
        }

        public async Task<SeasonDetails> GetSeasonEpisodes(int theMovieDbId, int seasonNumber, CancellationToken token, string langCode = "en")
        {
            var request = new Request($"/tv/{theMovieDbId}/season/{seasonNumber}", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            request.AddQueryString("language", langCode);
            AddRetry(request);

            return await Api.Request<SeasonDetails>(request, token);
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

        public async Task<List<Genre>> GetGenres(string media)
        {
            var request = new Request($"genre/{media}/list", BaseUri, HttpMethod.Get);
            request.AddQueryString("api_key", ApiToken);
            AddRetry(request);

            var result = await Api.Request<GenreContainer<Genre>>(request);
            return result.genres ?? new List<Genre>();
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

        private async Task AddDiscoverSettings(Request request)
        {
            var settings = await Settings;
            request.AddQueryString("include_adult", settings.ShowAdultMovies.ToString().ToLower());
            if (settings.ExcludedKeywordIds?.Any() == true)
            {
                request.AddQueryString("without_keywords", string.Join(",", settings.ExcludedKeywordIds));
            }
        }

        private async Task AddGenreFilter(Request request, string media_type)
        {
            var settings = await Settings;
            List<int> excludedGenres;

            switch (media_type) {
                case "tv":
                    excludedGenres = settings.ExcludedTvGenreIds;
                    break;
                case "movie":
                    excludedGenres = settings.ExcludedMovieGenreIds;
                    break;
                default:
                    return;
            }

            if (excludedGenres?.Any() == true)
            {
                request.AddQueryString("without_genres", string.Join(",", excludedGenres));
            }
        }

        private static void AddRetry(Request request)
        {
            request.Retry = true;
            request.StatusCodeToRetry.Add((HttpStatusCode)429);
        }
    }
}
