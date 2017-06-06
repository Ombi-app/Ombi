using Ombi.Core.Models.Requests;
using Ombi.Core.Rule;
using System.Collections.Generic;
using Ombi.Core.Models.Search;

namespace Ombi.Core.Rules
{
    public interface IRuleEvaluator
    {
        IEnumerable<RuleResult> StartRequestRules(BaseRequestModel obj);
        IEnumerable<RuleResult> StartSearchRules(SearchViewModel obj);
    }
}