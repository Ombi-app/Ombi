using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Core.Models.Search.V2;

namespace Ombi.Core
{
    public interface ITVSearchEngineV2
    {
        Task<SearchFullInfoTvShowViewModel> GetShowInformation(int tvdbid);
        Task<SearchFullInfoTvShowViewModel> GetShowByRequest(int requestId);
        Task<IEnumerable<StreamingData>> GetStreamInformation(int tvDbId, int tvMazeId, CancellationToken cancellationToken);
    }
}