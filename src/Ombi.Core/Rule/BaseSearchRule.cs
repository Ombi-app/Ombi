namespace Ombi.Core.Rule
{
    public abstract class BaseSearchRule
    {
        public RuleResult Success()
        {
            return new RuleResult {Success = true};
        }

        public RuleResult Fail(string message)
        {
            return new RuleResult {Message = message};
        }
    }
}