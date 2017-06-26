using Ombi.Core.Models.Requests;
using Ombi.Core.Rule;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rules
{
    public interface IRuleEvaluator
    {
        Task<IEnumerable<RuleResult>> StartRequestRules(BaseRequest obj);
        Task<IEnumerable<RuleResult>> StartSearchRules(SearchViewModel obj);
    }
}