using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ombi.Attributes;
using Ombi.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Controllers.V1
{
    [ApiV1]
    [Admin]
    [Produces("application/json")]
    [ApiController]
    public class RequestRetryController : Controller
    {
        public RequestRetryController(IRepository<RequestQueue> requestQueue, IMovieRequestRepository movieRepo,
            ITvRequestRepository tvRepo, IMusicRequestRepository musicRepo)
        {
            _requestQueueRepository = requestQueue;
            _movieRequestRepository = movieRepo;
            _tvRequestRepository = tvRepo;
            _musicRequestRepository = musicRepo;
        }

        private readonly IRepository<RequestQueue> _requestQueueRepository;
        private readonly IMovieRequestRepository _movieRequestRepository;
        private readonly ITvRequestRepository _tvRequestRepository;
        private readonly IMusicRequestRepository _musicRequestRepository;

        /// <summary>
        /// Get's all the failed requests that are currently in the queue
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<FailedRequestViewModel>> GetFailedRequests()
        {
            var failed = await _requestQueueRepository.GetAll().Where(x => !x.Completed.HasValue).ToListAsync();

            var vm = new List<FailedRequestViewModel>();
            foreach (var f in failed)
            {
                var vmModel = new FailedRequestViewModel
                {
                    RequestId = f.RequestId,
                    RetryCount = f.RetryCount,
                    Dts = f.Dts,
                    Error = f.Error,
                    FailedId = f.Id,
                    Type = f.Type
                };

                if (f.Type == RequestType.Movie)
                {
                    var request = await _movieRequestRepository.Find(f.RequestId);
                    vmModel.Title = request.Title;
                    vmModel.ReleaseYear = request.ReleaseDate;
                }

                if (f.Type == RequestType.Album)
                {
                    var request = await _musicRequestRepository.Find(f.RequestId);
                    vmModel.Title = request.Title;
                    vmModel.ReleaseYear = request.ReleaseDate;
                }

                if (f.Type == RequestType.TvShow)
                {
                    var request = await _tvRequestRepository.GetChild().Include(x => x.ParentRequest).FirstOrDefaultAsync(x => x.Id == f.RequestId);
                    vmModel.Title = request.Title;
                    vmModel.ReleaseYear = request.ParentRequest.ReleaseDate;
                }
                vm.Add(vmModel);
            }

            return vm;
        }

        [HttpDelete("{queueId:int}")]
        public async Task<IActionResult> Delete(int queueId)
        {
            var queueItem = await _requestQueueRepository.GetAll().FirstOrDefaultAsync(x => x.Id == queueId);
            await _requestQueueRepository.Delete(queueItem);
            return Json(true);
        }

    }
}