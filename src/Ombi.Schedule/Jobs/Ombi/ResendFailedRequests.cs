using System.Threading.Tasks;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class ResendFailedRequests
    {
        public ResendFailedRequests(IRepository<RequestQueue> queue, IMovieSender movieSender)
        {
            
        }

        public async Task Start()
        {

        }
    }
}