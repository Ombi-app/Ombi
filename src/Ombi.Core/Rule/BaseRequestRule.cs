using Ombi.Core.Engine;
namespace Ombi.Core.Rule
{
    public abstract class BaseRequestRule
    {
        public RuleResult Success()
        {
            return new RuleResult {Success = true};
        }

        public RuleResult Fail(ErrorCode errorCode, string message = "")
        {
            return new RuleResult { ErrorCode = errorCode, Message = message };
        }
    }
}