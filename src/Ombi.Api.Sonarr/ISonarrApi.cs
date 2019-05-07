using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Sonarr.Models;
using System.Net.Http;

namespace Ombi.Api.Sonarr
{
    public interface ISonarrApi
    {
        Task<IEnumerable<SonarrProfile>> GetProfiles(string apiKey, string baseUrl);
        Task<IEnumerable<SonarrRootFolder>> GetRootFolders(string apiKey, string baseUrl);
        Task<IEnumerable<SonarrSeries>> GetSeries(string apiKey, string baseUrl);
        Task<SonarrSeries> GetSeriesById(int id, string apiKey, string baseUrl);
        Task<SonarrSeries> UpdateSeries(SonarrSeries updated, string apiKey, string baseUrl);
        Task<NewSeries> AddSeries(NewSeries seriesToAdd, string apiKey, string baseUrl);
        Task<IEnumerable<Episode>> GetEpisodes(int seriesId, string apiKey, string baseUrl);
        Task<Episode> GetEpisodeById(int episodeId, string apiKey, string baseUrl);
        Task<EpisodeUpdateResult> UpdateEpisode(Episode episodeToUpdate, string apiKey, string baseUrl);
        Task<bool> EpisodeSearch(int[] episodeIds, string apiKey, string baseUrl);
        Task<bool> SeasonSearch(int seriesId, int seasonNumber, string apiKey, string baseUrl);
        Task<bool> SeriesSearch(int seriesId, string apiKey, string baseUrl);
        Task<SystemStatus> SystemStatus(string apiKey, string baseUrl);
        Task<bool> SeasonPass(string apiKey, string baseUrl, SonarrSeries series);
        Task<List<Tag>> GetTags(string apiKey, string baseUrl);
    }
}