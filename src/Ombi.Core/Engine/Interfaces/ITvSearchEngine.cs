using Ombi.Core.Models.Search;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ombi.Core.Engine.Interfaces
{
    public interface ITvSearchEngine
    {
        Task<IEnumerable<SearchTvShowViewModel>> Search(string searchTerm);
        Task<SearchTvShowViewModel> GetShowInformation(int tvdbid);
        Task<IEnumerable<SearchTvShowViewModel>> Popular();
        Task<IEnumerable<SearchTvShowViewModel>> Anticipated();
        Task<IEnumerable<SearchTvShowViewModel>> MostWatches();
        Task<IEnumerable<SearchTvShowViewModel>> Trending();
    }
}