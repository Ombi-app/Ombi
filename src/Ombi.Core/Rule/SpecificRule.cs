using Ombi.Core.Rule.Interfaces;

namespace Ombi.Core.Rule
{
    public abstract class SpecificRule
    {
        public RuleResult Success()
        {
            return new RuleResult { Success = true };
        }

        public RuleResult Fail(string message)
        {
            return new RuleResult { Message = message };
        }
        
        public abstract SpecificRules Rule { get; }
    }
}