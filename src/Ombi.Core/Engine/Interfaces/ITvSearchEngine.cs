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
        Task<IEnumerable<SearchTvShowViewModel>> Popular(int currentlyLoaded, int amountToLoad, bool includeImages = false);
        Task<IEnumerable<SearchTvShowViewModel>> Anticipated();
        Task<IEnumerable<SearchTvShowViewModel>> Anticipated(int currentlyLoaded, int amountToLoad);
        Task<IEnumerable<SearchTvShowViewModel>> Trending();
        Task<IEnumerable<SearchTvShowViewModel>> Trending(int currentlyLoaded, int amountToLoad);
        int ResultLimit { get; set; }
    }
}