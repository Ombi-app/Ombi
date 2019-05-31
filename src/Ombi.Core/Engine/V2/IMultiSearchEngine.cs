using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Api.TheMovieDb.Models;

namespace Ombi.Core.Engine.V2
{
    public interface IMultiSearchEngine
    {
        Task<List<MultiSearch>> MultiSearch(string searchTerm, CancellationToken cancellationToken, string lang = "en");
    }
}