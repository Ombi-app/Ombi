using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Radarr.Models;
using Ombi.Api.Radarr.Models.V3;

namespace Ombi.Api.Radarr
{
    public interface IRadarrV3Api
    {
        Task<List<MovieResponse>> GetMovies(string apiKey, string baseUrl);
        Task<List<RadarrV3QualityProfile>> GetProfiles(string apiKey, string baseUrl);
        Task<List<RadarrRootFolder>> GetRootFolders(string apiKey, string baseUrl);
        Task<SystemStatus> SystemStatus(string apiKey, string baseUrl);
        Task<MovieResponse> GetMovie(int id, string apiKey, string baseUrl);
        Task<MovieResponse> UpdateMovie(MovieResponse movie, string apiKey, string baseUrl);
        Task<bool> MovieSearch(int[] movieIds, string apiKey, string baseUrl);
        Task<RadarrAddMovie> AddMovie(int tmdbId, string title, int year, int qualityId, string rootPath,string apiKey, string baseUrl, bool searchNow, string minimumAvailability);
        Task<List<Tag>> GetTags(string apiKey, string baseUrl);
    }
}