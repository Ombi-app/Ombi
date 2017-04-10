using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;

namespace Ombi.Core
{
    public interface IMovieEngine
    {
        Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies();
        Task<IEnumerable<SearchMovieViewModel>> PopularMovies();
        Task<IEnumerable<SearchMovieViewModel>> ProcessMovieSearch(string search);
        Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies();
        Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies();
        Task<IEnumerable<SearchMovieViewModel>> LookupImdbInformation(IEnumerable<SearchMovieViewModel> movies);
    }
}