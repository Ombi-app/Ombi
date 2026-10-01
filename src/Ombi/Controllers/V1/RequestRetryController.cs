using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ombi.Attributes;
using Ombi.Core;
using Ombi.Core.Engine;
using Ombi.Core.Senders;
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
            ITvRequestRepository tvRepo, IMusicRequestRepository musicRepo, IMovieSender movieSender,
            ITvSender tvSender, IMusicSender musicSender)
        {
            _requestQueueRepository = requestQueue;
            _movieRequestRepository = movieRepo;
            _tvRequestRepository = tvRepo;
            _musicRequestRepository = musicRepo;
            _movieSender = movieSender;
            _tvSender = tvSender;
            _musicSender = musicSender;
        }

        private readonly IRepository<RequestQueue> _requestQueueRepository;
        private readonly IMovieRequestRepository _movieRequestRepository;
        private readonly ITvRequestRepository _tvRequestRepository;
        private readonly IMusicRequestRepository _musicRequestRepository;
        private readonly IMovieSender _movieSender;
        private readonly ITvSender _tvSender;
        private readonly IMusicSender _musicSender;

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
                    if (request == null)
                    {
                        // The request may have been deleted while an old retry row still exists.
                        // Do not let one stale queue entry break the entire Failed Requests page;
                        // ResendFailedRequests will remove orphaned queue rows on its next run.
                        continue;
                    }
                    vmModel.Title = request.Title;
                    vmModel.ReleaseYear = request.ReleaseDate;
                }

                if (f.Type == RequestType.Album)
                {
                    var request = await _musicRequestRepository.Find(f.RequestId);
                    if (request == null)
                    {
                        continue;
                    }
                    vmModel.Title = request.Title;
                    vmModel.ReleaseYear = request.ReleaseDate;
                }

                if (f.Type == RequestType.TvShow)
                {
                    var request = await _tvRequestRepository.GetChild().Include(x => x.ParentRequest).FirstOrDefaultAsync(x => x.Id == f.RequestId);
                    if (request?.ParentRequest == null)
                    {
                        continue;
                    }
                    vmModel.Title = request.Title;
                    vmModel.ReleaseYear = request.ParentRequest.ReleaseDate;
                }
                vm.Add(vmModel);
            }

            return vm;
        }

        /// <summary>
        /// Manually retry a failed request queue entry.
        /// </summary>
        /// <param name="queueId">The failed request queue identifier.</param>
        [HttpPost("{queueId:int}/retry")]
        public async Task<RequestEngineResult> Retry(int queueId)
        {
            var queueItem = await _requestQueueRepository.GetAll()
                .FirstOrDefaultAsync(x => x.Id == queueId && !x.Completed.HasValue);
            if (queueItem == null)
            {
                return MissingRequestResult();
            }

            SenderResult result;
            switch (queueItem.Type)
            {
                case RequestType.Movie:
                {
                    var request = await _movieRequestRepository.GetWithUser()
                        .FirstOrDefaultAsync(x => x.Id == queueItem.RequestId);
                    if (request == null)
                    {
                        return MissingRequestResult(queueItem.RequestId);
                    }

                    // RequestQueue does not currently record whether a movie failure originated
                    // from the normal or 4K path. Match the existing scheduled retry behavior by
                    // using the request's approved 4K state until that queue schema can distinguish it.
                    result = await _movieSender.Send(request, request.Approved4K);
                    break;
                }
                case RequestType.TvShow:
                {
                    var request = await _tvRequestRepository.GetChild()
                        .FirstOrDefaultAsync(x => x.Id == queueItem.RequestId);
                    if (request == null)
                    {
                        return MissingRequestResult(queueItem.RequestId);
                    }

                    result = await _tvSender.Send(request);
                    break;
                }
                case RequestType.Album:
                {
                    var request = await _musicRequestRepository.GetAll()
                        .FirstOrDefaultAsync(x => x.Id == queueItem.RequestId);
                    if (request == null)
                    {
                        return MissingRequestResult(queueItem.RequestId);
                    }

                    result = await _musicSender.Send(request);
                    break;
                }
                default:
                    return new RequestEngineResult
                    {
                        Result = false,
                        ErrorMessage = "Unsupported request type",
                        RequestId = queueItem.RequestId
                    };
            }

            if (result?.Success == true)
            {
                queueItem.Completed = DateTime.UtcNow;
                await _requestQueueRepository.SaveChangesAsync();

                return new RequestEngineResult
                {
                    Result = true,
                    RequestId = queueItem.RequestId
                };
            }

            return new RequestEngineResult
            {
                Result = false,
                ErrorMessage = result?.Message ?? "Re-processing failed",
                RequestId = queueItem.RequestId
            };
        }

        [HttpDelete("{queueId:int}")]
        public async Task<IActionResult> Delete(int queueId)
        {
            var queueItem = await _requestQueueRepository.GetAll().FirstOrDefaultAsync(x => x.Id == queueId);
            await _requestQueueRepository.Delete(queueItem);
            return Json(true);
        }

        private static RequestEngineResult MissingRequestResult(int requestId = 0)
        {
            return new RequestEngineResult
            {
                Result = false,
                ErrorCode = ErrorCode.RequestDoesNotExist,
                ErrorMessage = "Request does not exist",
                RequestId = requestId
            };
        }

    }
}
