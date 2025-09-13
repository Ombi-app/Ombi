using System.Threading.Tasks;
using Ombi.Api.External.ExternalApis.DogNzb.Models;

namespace Ombi.Api.External.ExternalApis.DogNzb
{
    public interface IDogNzbApi
    {
        Task<DogNzbMovieAddResult> AddMovie(string apiKey, string imdbid);
        Task<DogNzbAddResult> AddTvShow(string apiKey, string tvdbId);
        Task<DogNzbMovies> ListMovies(string apiKey);
        Task<DogNzbTvShows> ListTvShows(string apiKey);
    }
}