using Ombi.Core.Models.Requests;

namespace Ombi.Core.Rule.Interfaces
{
    public interface IRequestRules<T> where T : new() 
    {
        RuleResult Execute(T obj);
    }
}