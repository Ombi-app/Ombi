using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;

namespace Ombi.Core.Engine
{
    public interface ITvSearchEngine
    {
        Task<IEnumerable<SearchTvShowViewModel>> Search(string searchTerm);
    }
}