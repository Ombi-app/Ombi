using System.Threading.Tasks;
using Ombi.Api.DogNzb.Models;

namespace Ombi.Api.DogNzb
{
    public interface IDogNzbApi
    {
        Task<DogNzbMovieAddResult> AddMovie(string apiKey, string imdbid);
        Task<DogNzbAddResult> AddTvShow(string apiKey, string tvdbId);
        Task<DogNzbMovies> ListMovies(string apiKey);
        Task<DogNzbTvShows> ListTvShows(string apiKey);
    }
}