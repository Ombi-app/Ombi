using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;

namespace Ombi.Core.Engine.Interfaces
{
    public interface IMovieEngineV2
    {
        Task<MovieFullInfoViewModel> GetFullMovieInformation(int theMovieDbId, string langCode = null);
        Task<IEnumerable<SearchMovieViewModel>> SimilarMovies(int theMovieDbId, string langCode);
        Task<IEnumerable<SearchMovieViewModel>> PopularMovies();
        Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies();
        Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies();
        Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies();
        Task<int> GetTvDbId(int theMovieDbId);
        int ResultLimit { get; set; }
    }
}