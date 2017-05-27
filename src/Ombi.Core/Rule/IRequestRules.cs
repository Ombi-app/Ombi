using Ombi.Core.Models.Requests;

namespace Ombi.Core.Rules
{
    public interface IRequestRules<T> where T : BaseRequestModel
    {
        RuleResult Execute(T obj);
    }
}
