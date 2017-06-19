using Ombi.Core.Models.Requests;
using Ombi.Core.Rule;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;

namespace Ombi.Core.Rules
{
    public interface IRuleEvaluator
    {
        Task<IEnumerable<RuleResult>> StartRequestRules(BaseRequestModel obj);
        Task<IEnumerable<RuleResult>> StartSearchRules(SearchViewModel obj);
    }
}