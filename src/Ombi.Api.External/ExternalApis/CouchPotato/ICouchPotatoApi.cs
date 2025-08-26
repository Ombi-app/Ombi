using System.Threading.Tasks;
using Ombi.Api.External.ExternalApis.CouchPotato.Models;

namespace Ombi.Api.External.ExternalApis.CouchPotato
{
    public interface ICouchPotatoApi
    {
        Task<bool> AddMovie(string imdbid, string apiKey, string title, string baseUrl, string profileId = null);
        Task<CouchPotatoApiKey> GetApiKey(string baseUrl, string username, string password);
        Task<CouchPotatoMovies> GetMovies(string baseUrl, string apiKey, string[] status);
        Task<CouchPotatoProfiles> GetProfiles(string url, string apiKey);
        Task<CouchPotatoStatus> Status(string url, string apiKey);
    }
}