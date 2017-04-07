using System.Threading.Tasks;
using Ombi.Api;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.TheMovieDbApi
{
    public interface IMovieDbApi
    {
        Task<MovieResponse> GetMovieInformation(int movieId);
        Task<MovieResponse> GetMovieInformationWithVideo(int movieId);
        Task<TheMovieDbContainer<SearchResult>> NowPlaying();
        Task<TheMovieDbContainer<SearchResult>> PopularMovies();
        Task<TheMovieDbContainer<SearchResult>> SearchMovie(string searchTerm);
        Task<TheMovieDbContainer<SearchResult>> TopRated();
        Task<TheMovieDbContainer<SearchResult>> Upcoming();
    }
}