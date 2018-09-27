using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.TheMovieDb.Models;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.Api.TheMovieDb
{
    public interface IMovieDbApi
    {
        Task<MovieResponseDto> GetMovieInformation(int movieId);
        Task<MovieResponseDto> GetMovieInformationWithExtraInfo(int movieId);
        Task<List<MovieSearchResult>> NowPlaying();
        Task<List<MovieSearchResult>> PopularMovies();
        Task<List<MovieSearchResult>> SearchMovie(string searchTerm);
        Task<List<TvSearchResult>> SearchTv(string searchTerm);
        Task<List<MovieSearchResult>> TopRated();
        Task<List<MovieSearchResult>> Upcoming();
        Task<List<MovieSearchResult>> SimilarMovies(int movieId);
        Task<FindResult> Find(string externalId, ExternalSource source);
        Task<TvExternals> GetTvExternals(int theMovieDbId);
        Task<TvInfo> GetTVInfo(string themoviedbid);
    }
}