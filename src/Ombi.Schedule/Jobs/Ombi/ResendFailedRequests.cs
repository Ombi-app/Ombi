using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core;
using Ombi.Core.Senders;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Quartz;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class ResendFailedRequests : IResendFailedRequests
    {
        public ResendFailedRequests(IRepository<RequestQueue> queue, IMovieSender movieSender, ITvSender tvSender, IMusicSender musicSender,
            IMovieRequestRepository movieRepo, ITvRequestRepository tvRepo, IMusicRequestRepository music)
        {
            _requestQueue = queue;
            _movieSender = movieSender;
            _tvSender = tvSender;
            _musicSender = musicSender;
            _movieRequestRepository = movieRepo;
            _tvRequestRepository = tvRepo;
            _musicRequestRepository = music;
        }

        private readonly IRepository<RequestQueue> _requestQueue;
        private readonly IMovieSender _movieSender;
        private readonly ITvSender _tvSender;
        private readonly IMusicSender _musicSender;
        private readonly IMovieRequestRepository _movieRequestRepository;
        private readonly ITvRequestRepository _tvRequestRepository;
        private readonly IMusicRequestRepository _musicRequestRepository;

        private const int MaxRetryLimit = 10;

        public async Task Execute(IJobExecutionContext job)
        {
            var cancellationToken = job?.CancellationToken ?? CancellationToken.None;

            // Get all the failed ones!
            var failedRequests = await _requestQueue.GetAll().Where(x => x.Completed == null).ToListAsync(cancellationToken);

            foreach (var request in failedRequests)
            {
                // Abandon items exceeding max retries or carrying unretryable metadata errors
                if (request.RetryCount >= MaxRetryLimit ||
                    (request.Type == RequestType.TvShow && !string.IsNullOrEmpty(request.Error) && request.Error.Contains("TVDBID is missing", StringComparison.OrdinalIgnoreCase)))
                {
                    await _requestQueue.Delete(request);
                    await _requestQueue.SaveChangesAsync();
                    continue;
                }

                if (request.Type == RequestType.Movie)
                {
                    var movieRequest = await _movieRequestRepository.GetAll().FirstOrDefaultAsync(x => x.Id == request.RequestId, cancellationToken);
                    if (movieRequest == null)
                    {
                        await _requestQueue.Delete(request);
                        await _requestQueue.SaveChangesAsync();
                        continue;
                    }

                    // TODO probably need to add something to the request queue to better idenitfy if it's a 4k request
                    var result = await _movieSender.Send(movieRequest, movieRequest.Approved4K);
                    await HandleRetryResultAsync(request, result);
                }
                if (request.Type == RequestType.TvShow)
                {
                    var tvRequest = await _tvRequestRepository.GetChild().FirstOrDefaultAsync(x => x.Id == request.RequestId, cancellationToken);
                    if (tvRequest == null)
                    {
                        await _requestQueue.Delete(request);
                        await _requestQueue.SaveChangesAsync();
                        continue;
                    }
                    var result = await _tvSender.Send(tvRequest);
                    await HandleRetryResultAsync(request, result);
                }
                if (request.Type == RequestType.Album)
                {
                    var musicRequest = await _musicRequestRepository.GetAll().FirstOrDefaultAsync(x => x.Id == request.RequestId, cancellationToken);
                    if (musicRequest == null)
                    {
                        await _requestQueue.Delete(request);
                        await _requestQueue.SaveChangesAsync();
                        continue;
                    }
                    var result = await _musicSender.Send(musicRequest);
                    await HandleRetryResultAsync(request, result);
                }
            }
        }

        private async Task HandleRetryResultAsync(RequestQueue request, SenderResult result)
        {
            if (result?.Success == true)
            {
                request.Completed = DateTime.UtcNow;
            }
            else
            {
                request.RetryCount++;
                if (result != null && !string.IsNullOrEmpty(result.Message))
                {
                    request.Error = result.Message;
                }
            }
            await _requestQueue.SaveChangesAsync();
        }
    }
}