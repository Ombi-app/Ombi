using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;

namespace Ombi.Core.Engine.Interfaces
{
    public interface IRequestEngine<T>
    {

        //Task<IEnumerable<T>> GetApprovedRequests();
        //Task<IEnumerable<T>> GetNewRequests();
        //Task<IEnumerable<T>> GetAvailableRequests();
        RequestCountModel RequestCount();
        Task<IEnumerable<T>> GetRequests(int count, int position);
        Task<IEnumerable<T>> GetRequests();
    }
}