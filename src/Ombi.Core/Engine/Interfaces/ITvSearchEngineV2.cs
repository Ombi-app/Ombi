using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Core.Models.Search.V2;

namespace Ombi.Core
{
    public interface ITVSearchEngineV2
    {
        Task<SearchFullInfoTvShowViewModel> GetShowInformation(string tvdbid, CancellationToken token);
        Task<SearchFullInfoTvShowViewModel> GetShowByRequest(int requestId, CancellationToken token);
        Task<IEnumerable<StreamingData>> GetStreamInformation(int movieDbId, CancellationToken cancellationToken);
    }
}