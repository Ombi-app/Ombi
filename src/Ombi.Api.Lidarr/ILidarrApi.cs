using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Lidarr.Models;

namespace Ombi.Api.Lidarr
{
    public interface ILidarrApi
    {
        Task<List<AlbumLookup>> AlbumLookup(string searchTerm, string apiKey, string baseUrl);
        Task<List<AlbumLookup>> ArtistLookup(string searchTerm, string apiKey, string baseUrl);
        Task<List<LidarrProfile>> GetProfiles(string apiKey, string baseUrl);
        Task<List<LidarrRootFolder>> GetRootFolders(string apiKey, string baseUrl);
    }
}