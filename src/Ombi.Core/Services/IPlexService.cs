using Ombi.Core.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Core.Services
{
    public interface IPlexService
    {
        Task<List<PlexUserWatchlistModel>> GetWatchlistUsers(CancellationToken cancellationToken);
    }
}
