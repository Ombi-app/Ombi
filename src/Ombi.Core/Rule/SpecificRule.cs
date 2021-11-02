using Ombi.Core.Engine;
using Ombi.Core.Rule.Interfaces;

namespace Ombi.Core.Rule
{
    public abstract class SpecificRule
    {
        public RuleResult Success()
        {
            return new RuleResult { Success = true };
        }

        public RuleResult Fail(ErrorCode errorCode, string message = "")
        {
            return new RuleResult {  ErrorCode = errorCode, Message = message };
        }
        
        public abstract SpecificRules Rule { get; }
    }
}