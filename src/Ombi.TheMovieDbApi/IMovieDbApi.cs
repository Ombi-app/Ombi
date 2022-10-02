using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Api.TheMovieDb.Models;
using Ombi.TheMovieDbApi.Models;

// Due to conflicting Genre models in
// Ombi.TheMovieDbApi.Models and Ombi.Api.TheMovieDb.Models   
using Genre = Ombi.TheMovieDbApi.Models.Genre;

namespace Ombi.Api.TheMovieDb
{
    public interface IMovieDbApi
    {
        Task<MovieResponseDto> GetMovieInformation(int movieId);
        Task<MovieResponseDto> GetMovieInformationWithExtraInfo(int movieId, string langCode = "en");
        Task<List<MovieDbSearchResult>> NowPlaying(string languageCode, int? page = null);
        Task<List<MovieDbSearchResult>> TrendingMovies(string languageCode, int? page = null);
        Task<List<MovieDbSearchResult>> PopularMovies(string languageCode, int? page = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<MovieDbSearchResult>> PopularTv(string langCode, int? page = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<MovieDbSearchResult>> SearchMovie(string searchTerm, int? year, string languageCode);
        Task<List<MovieDbSearchResult>> GetMoviesViaKeywords(string keywordId, string langCode, CancellationToken cancellationToken, int? page = null);
        Task<List<TvSearchResult>> SearchTv(string searchTerm, string year = default);
        Task<List<MovieDbSearchResult>> TopRated(string languageCode, int? page = null);
        Task<List<MovieDbSearchResult>> UpcomingMovies(string languageCode, int? page = null);
        Task<List<MovieDbSearchResult>> TopRatedTv(string languageCode, int? page = null);
        Task<List<MovieDbSearchResult>> TrendingTv(string languageCode, int? page = null);
        Task<List<MovieDbSearchResult>> UpcomingTv(string languageCode, int? page = null);
        Task<List<MovieDbSearchResult>> SimilarMovies(int movieId, string langCode);
        Task<FindResult> Find(string externalId, ExternalSource source);
        Task<TvExternals> GetTvExternals(int theMovieDbId);
        Task<SeasonDetails> GetSeasonEpisodes(int theMovieDbId, int seasonNumber, CancellationToken token, string langCode = "en");
        Task<TvInfo> GetTVInfo(string themoviedbid, string langCode = "en");
        Task<TheMovieDbContainer<ActorResult>> SearchByActor(string searchTerm, string langCode);
        Task<ActorCredits> GetActorMovieCredits(int actorId, string langCode);
        Task<ActorCredits> GetActorTvCredits(int actorId, string langCode);
        Task<TheMovieDbContainer<MultiSearch>> MultiSearch(string searchTerm, string languageCode, CancellationToken cancellationToken);
        Task<TheMovieDbContainer<DiscoverMovies>> DiscoverMovies(string langCode, int keywordId);
        Task<FullMovieInfo> GetFullMovieInfo(int movieId, CancellationToken cancellationToken, string langCode);
        Task<Collections> GetCollection(string langCode, int collectionId, CancellationToken cancellationToken);
        Task<List<TheMovidDbKeyValue>> SearchKeyword(string searchTerm);
        Task<TheMovidDbKeyValue> GetKeyword(int keywordId);
        Task<WatchProviders> GetMovieWatchProviders(int theMoviedbId, CancellationToken token);
        Task<WatchProviders> GetTvWatchProviders(int theMoviedbId, CancellationToken token);
        Task<List<Genre>> GetGenres(string media, CancellationToken cancellationToken, string languageCode);
        Task<List<Language>> GetLanguages(CancellationToken cancellationToken);
        Task<List<WatchProvidersResults>> SearchWatchProviders(string media, string searchTerm, CancellationToken cancellationToken);
        Task<List<MovieDbSearchResult>> AdvancedSearch(DiscoverModel model, int page, CancellationToken cancellationToken);
        Task<MovieDbImages> GetTvImages(string theMovieDbId, CancellationToken token);
        Task<MovieDbImages> GetMovieImages(string theMovieDbId, CancellationToken token);
    }
}
