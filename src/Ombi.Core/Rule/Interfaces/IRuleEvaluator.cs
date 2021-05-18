using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rule.Interfaces
{
    public interface IRuleEvaluator
    {
        Task<IEnumerable<RuleResult>> StartRequestRules(BaseRequest obj);
        Task<IEnumerable<RuleResult>> StartSearchRules(SearchViewModel obj);
        Task<RuleResult> StartSpecificRules(object obj, SpecificRules selectedRule, string requestOnBehalf);
    }
}