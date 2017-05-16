using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Sonarr.Models;

namespace Ombi.Api.Sonarr
{
    public interface ISonarrApi
    {
        Task<IEnumerable<SonarrProfile>> GetProfiles(string apiKey, string baseUrl);
        Task<IEnumerable<SonarrRootFolder>> GetRootFolders(string apiKey, string baseUrl);
    }
}