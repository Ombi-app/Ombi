using System.Threading.Tasks;

namespace Ombi.Core.Rule.Interfaces
{
    public interface IRules<T> 
    {
        Task<RuleResult> Execute(T obj);
    }
}