using Ombi.Core.Rules;

namespace Ombi.Core.Rule
{
    public abstract class BaseRule
    {
        public RuleResult Success()
        {
            return new RuleResult { Success = true };
        }

        public RuleResult Fail(string message)
        {
            return new RuleResult { Message = message };
        }
    }
}
