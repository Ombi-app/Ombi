using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Radarr.Models;

namespace Ombi.Api.Radarr
{
    public interface IRadarrApi
    {
        Task<List<MovieResponse>> GetMovies(string apiKey, Uri baseUrl);
        Task<List<RadarrProfile>> GetProfiles(string apiKey, Uri baseUrl);
        Task<List<RadarrRootFolder>> GetRootFolders(string apiKey, Uri baseUrl);
        Task<SystemStatus> SystemStatus(string apiKey, Uri baseUrl);
    }
}