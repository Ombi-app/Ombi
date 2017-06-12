using Ombi.Core.Models.Search;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ombi.Core
{
    public interface IMovieEngine
    {
        Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies();

        Task<IEnumerable<SearchMovieViewModel>> PopularMovies();

        Task<IEnumerable<SearchMovieViewModel>> Search(string search);

        Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies();

        Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies();

        Task<IEnumerable<SearchMovieViewModel>> LookupImdbInformation(IEnumerable<SearchMovieViewModel> movies);
        Task<SearchMovieViewModel> LookupImdbInformation(int theMovieDbId);
    }
}