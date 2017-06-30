using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Sonarr.Models;

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
    }
}