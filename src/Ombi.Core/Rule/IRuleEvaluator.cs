using System.Collections.Generic;
using Ombi.Core.Models.Requests;

namespace Ombi.Core.Rules
{
    public interface IRuleEvaluator
    {
        IEnumerable<RuleResult> StartRequestRules(BaseRequestModel obj);
    }
}