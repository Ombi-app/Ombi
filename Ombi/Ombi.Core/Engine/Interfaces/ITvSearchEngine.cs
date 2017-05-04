using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;

namespace Ombi.Core.Engine
{
    public interface ITvSearchEngine
    {
        Task<IEnumerable<SearchTvShowViewModel>> Search(string searchTerm);
        //Task<IEnumerable<SearchTvShowViewModel>> Popular();
        //Task<IEnumerable<SearchTvShowViewModel>> Anticipated();
        //Task<IEnumerable<SearchTvShowViewModel>> MostWatches();
        //Task<IEnumerable<SearchTvShowViewModel>> Trending();
    }
}