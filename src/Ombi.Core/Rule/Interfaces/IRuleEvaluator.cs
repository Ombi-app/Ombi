using Ombi.Core.Models.Requests;
using Ombi.Core.Rule;
using System.Collections.Generic;

namespace Ombi.Core.Rules
{
    public interface IRuleEvaluator
    {
        IEnumerable<RuleResult> StartRequestRules(BaseRequestModel obj);
    }
}