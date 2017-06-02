using Ombi.Core.Models.Requests;
using Ombi.Core.Rule;

namespace Ombi.Core.Rules
{
    public interface IRequestRules<T> where T : BaseRequestModel
    {
        RuleResult Execute(T obj);
    }
}