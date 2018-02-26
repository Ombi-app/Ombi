using Ombi.Core.Models.Search;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ombi.Core.Engine.Interfaces
{
    public interface ITvSearchEngine
    {
        Task<IEnumerable<SearchTvShowViewModel>> Search(string searchTerm);
        Task<IEnumerable<TreeNode<SearchTvShowViewModel>>> SearchTreeNode(string searchTerm);
        Task<TreeNode<SearchTvShowViewModel>> GetShowInformationTreeNode(int tvdbid);
        Task<SearchTvShowViewModel> GetShowInformation(int tvdbid);
        Task<IEnumerable<TreeNode<SearchTvShowViewModel>>> PopularTree();
        Task<IEnumerable<SearchTvShowViewModel>> Popular();
        Task<IEnumerable<TreeNode<SearchTvShowViewModel>>> AnticipatedTree();
        Task<IEnumerable<SearchTvShowViewModel>> Anticipated();
        Task<IEnumerable<TreeNode<SearchTvShowViewModel>>> MostWatchesTree();
        Task<IEnumerable<SearchTvShowViewModel>> MostWatches();
        Task<IEnumerable<TreeNode<SearchTvShowViewModel>>> TrendingTree();
        Task<IEnumerable<SearchTvShowViewModel>> Trending();
    }
}