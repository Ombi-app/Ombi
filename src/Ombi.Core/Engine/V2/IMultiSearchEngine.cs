using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Models.Search.V2;

// Due to conflicting Genre models in
// Ombi.TheMovieDbApi.Models and Ombi.Api.TheMovieDb.Models   
using Genre = Ombi.TheMovieDbApi.Models.Genre;

namespace Ombi.Core.Engine.V2
{
    public interface IMultiSearchEngine
    {
        Task<List<MultiSearchResult>> MultiSearch(string searchTerm, MultiSearchFilter filter, CancellationToken cancellationToken);
        Task<IEnumerable<Genre>> GetGenres(string media, CancellationToken requestAborted);
    }
}