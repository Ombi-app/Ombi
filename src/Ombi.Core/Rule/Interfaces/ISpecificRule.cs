using System.Threading.Tasks;
using Ombi.Core.Models.Requests;

namespace Ombi.Core.Rule.Interfaces
{
    public interface ISpecificRule<T> where T : new() 
    {
        Task<RuleResult> Execute(T obj, string requestOnBehalf);
        SpecificRules Rule { get; }
    }
}