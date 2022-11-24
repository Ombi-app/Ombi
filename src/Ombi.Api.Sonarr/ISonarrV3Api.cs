using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Sonarr.Models;
using Ombi.Api.Sonarr.Models.V3;

namespace Ombi.Api.Sonarr
{
    public interface ISonarrV3Api : ISonarrApi
    {
        Task<IEnumerable<LanguageProfiles>> LanguageProfiles(string apiKey, string baseUrl);
        Task<Tag> CreateTag(string apiKey, string baseUrl, string tagName);
        Task<Tag> GetTag(int tagId, string apiKey, string baseUrl);
        Task<List<MonitoredEpisodeResult>> MonitorEpisode(int[] episodeIds, bool monitor, string apiKey, string baseUrl);
    }
}