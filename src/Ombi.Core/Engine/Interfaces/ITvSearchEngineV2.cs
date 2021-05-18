using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;

namespace Ombi.Core
{
    public interface ITVSearchEngineV2
    {
        Task<SearchFullInfoTvShowViewModel> GetShowInformation(string tvdbid, CancellationToken token);
        Task<SearchFullInfoTvShowViewModel> GetShowByRequest(int requestId, CancellationToken token);
        Task<IEnumerable<StreamingData>> GetStreamInformation(int movieDbId, CancellationToken cancellationToken);
        Task<IEnumerable<SearchTvShowViewModel>> Popular(int currentlyLoaded, int amountToLoad, string langCustomCode = null);
        Task<IEnumerable<SearchTvShowViewModel>> Anticipated(int currentlyLoaded, int amountToLoad);
        Task<IEnumerable<SearchTvShowViewModel>> Trending(int currentlyLoaded, int amountToLoad);
    }
}