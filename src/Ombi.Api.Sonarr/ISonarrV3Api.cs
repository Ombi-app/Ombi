using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Sonarr.Models;
using System.Net.Http;
using Ombi.Api.Sonarr.Models.V3;

namespace Ombi.Api.Sonarr
{
    public interface ISonarrV3Api : ISonarrApi
    {
        Task<IEnumerable<LanguageProfiles>> LanguageProfiles(string apiKey, string baseUrl);
    }
}