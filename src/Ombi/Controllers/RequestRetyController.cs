using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Entities;

namespace Ombi.Controllers
{
    [ApiV1]
    [Authorize]
    [Produces("application/json")]
    public class RequestRetyController : Controller
    {
        public RequestRetyController(IRepository<RequestQueue> requestQueue)
        {
            _requestQueueRepository = requestQueue;
        }

        private readonly IRepository<RequestQueue> _requestQueueRepository;

        /// <summary>
        /// Get's all the failed requests that are currently in the queue
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<RequestQueue>> Categories()
        {
            return await _requestQueueRepository.GetAll().Where(x => !x.Completed.HasValue).ToListAsync();
        }

        [HttpDelete("{queueId:int}")]
        public async Task<IActionResult> Delete(int queueId)
        {
            var queueItem = await _requestQueueRepository.GetAll().FirstOrDefaultAsync(x => x.Id == queueId);
            await _requestQueueRepository.Delete(queueItem);
            return Ok();
        }

    }
}