using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Models.Search.V2;

namespace Ombi.Core.Engine.V2
{
    public interface IMultiSearchEngine
    {
        Task<List<MultiSearchResult>> MultiSearch(string searchTerm, MultiSearchFilter filter, CancellationToken cancellationToken);
    }
}