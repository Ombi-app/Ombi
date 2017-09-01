using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class EmbyAvailabilityRule : BaseSearchRule, IRules<SearchViewModel>
    {
        public EmbyAvailabilityRule(IEmbyContentRepository repo)
        {
            EmbyContentRepository = repo;
        }

        private IEmbyContentRepository EmbyContentRepository { get; }

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            var item = await EmbyContentRepository.Get(obj.CustomId);
            if (item != null)
            {
                obj.Available = true;
            }
            return Success();
        }
    }
}