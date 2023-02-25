using Ombi.Core.Models.Requests;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Core.Services
{
    public interface IRecentlyRequestedService
    {
        Task<IEnumerable<RecentlyRequestedModel>> GetRecentlyRequested(CancellationToken cancellationToken);
    }
}
