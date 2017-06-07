using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Radarr.Models;

namespace Ombi.Api.Radarr
{
    public interface IRadarrApi
    {
        Task<List<MovieResponse>> GetMovies(string apiKey, string baseUrl);
        Task<List<RadarrProfile>> GetProfiles(string apiKey, string baseUrl);
        Task<List<RadarrRootFolder>> GetRootFolders(string apiKey, string baseUrl);
        Task<SystemStatus> SystemStatus(string apiKey, string baseUrl);
        Task<RadarrAddMovieResponse> AddMovie(int tmdbId, string title, int year, int qualityId, string rootPath,string apiKey, string baseUrl, bool searchNow = false);
    }
}