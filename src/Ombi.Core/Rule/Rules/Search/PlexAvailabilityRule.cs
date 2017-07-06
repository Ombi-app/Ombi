using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class PlexAvailabilityRule : BaseSearchRule, IRules<SearchViewModel>
    {
        public PlexAvailabilityRule(IPlexContentRepository repo)
        {
            PlexContentRepository = repo;
        }

        private IPlexContentRepository PlexContentRepository { get; }

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            var item = await PlexContentRepository.Get(obj.Id.ToString());
            if (item != null)
            {
                obj.Available = true;
                obj.PlexUrl = item.Url;
            }
            return Success();
        }
    }
}