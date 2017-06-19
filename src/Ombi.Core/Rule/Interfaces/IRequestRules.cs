using System.Threading.Tasks;
using Ombi.Core.Models.Requests;

namespace Ombi.Core.Rule.Interfaces
{
    public interface IRequestRules<T> where T : new() 
    {
        Task<RuleResult> Execute(T obj);
    }
}