using System.Threading.Tasks;
using Ombi.Api.SickRage.Models;

namespace Ombi.Api.SickRage
{
    public interface ISickRageApi
    {
        Task<SickRageTvAdd> AddSeason(int tvdbId, int season, string apiKey, string baseUrl);
        Task<SickRageTvAdd> AddSeries(int tvdbId, int seasonCount, int[] seasons, string quality, string apiKey, string baseUrl);
        Task<SickRageShows> GetShows(string apiKey, string baseUrl);
        Task<SickRagePing> Ping(string apiKey, string baseUrl);
        Task<SickRageSeasonList> VerifyShowHasLoaded(int tvdbId, string apiKey, string baseUrl);

        Task<SickRageEpisodeStatus> SetEpisodeStatus(string apiKey, string baseUrl, int tvdbid, string status,
            int season, int episode = -1);
    }
}