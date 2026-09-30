using System;
using System.Linq;
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
        private const int MissingTvDbMaxAutomaticRetries = 3;

        public async Task Execute(IJobExecutionContext job)
        {
            // Get all the failed ones!
            var failedRequests = _requestQueue.GetAll().Where(x => x.Completed == null);

            foreach (var request in failedRequests)
            {
                if (request.Type == RequestType.Movie)
                {
                    var movieRequest = await _movieRequestRepository.GetWithUser().FirstOrDefaultAsync(x => x.Id == request.RequestId);
                    if (movieRequest == null)
                    {
                        await _requestQueue.Delete(request);
                        await _requestQueue.SaveChangesAsync();
                        continue;
                    }

                    // TODO probably need to add something to the request queue to better idenitfy if it's a 4k request
                    var result = await _movieSender.Send(movieRequest, movieRequest.Approved4K);
                    if (result.Success)
                    {
                        request.Completed = DateTime.UtcNow;
                        await _requestQueue.SaveChangesAsync();
                    }
                }
                if (request.Type == RequestType.TvShow)
                {
                    var tvRequest = await _tvRequestRepository.GetChild().FirstOrDefaultAsync(x => x.Id == request.RequestId);
                    if (tvRequest == null)
                    {
                        await _requestQueue.Delete(request);
                        await _requestQueue.SaveChangesAsync();
                        continue;
                    }

                    // Deterministic mapping failures require an administrator to reconcile the
                    // Ombi/TMDB request with the existing Sonarr series. Keep the queue row
                    // incomplete so it remains visible under Failed Requests, but do not keep
                    // retrying an operation that cannot succeed without human intervention.
                    var manualInterventionRequired = request.Error?.StartsWith(
                        TvSender.ManualInterventionQueuePrefix,
                        StringComparison.OrdinalIgnoreCase) == true;
                    if (manualInterventionRequired)
                    {
                        continue;
                    }

                    // A TV request with no TVDB mapping can never be accepted by Sonarr.
                    // TvSender gets at least one chance after this patch to repair legacy rows from
                    // TMDB. If TMDB still has no mapping, stop retrying it every day after a small
                    // number of attempts. Keep the queue row incomplete so it remains visible under
                    // Failed Requests. If an admin later supplies a TVDB ID, automatic retries resume.
                    var unresolvedTvDb = tvRequest.ParentRequest?.TvDbId <= 0 &&
                        request.Error?.StartsWith(TvSender.MissingTvDbAfterRefreshPrefix, StringComparison.OrdinalIgnoreCase) == true;
                    if (unresolvedTvDb && request.RetryCount >= MissingTvDbMaxAutomaticRetries)
                    {
                        continue;
                    }

                    var result = await _tvSender.Send(tvRequest);
                    if (result.Success)
                    {
                        request.Completed = DateTime.UtcNow;
                        await _requestQueue.SaveChangesAsync();
                    }
                }
                if (request.Type == RequestType.Album)
                {
                    var musicRequest = await _musicRequestRepository.GetAll().FirstOrDefaultAsync(x => x.Id == request.RequestId);
                    if (musicRequest == null)
                    {
                        await _requestQueue.Delete(request);
                        await _requestQueue.SaveChangesAsync();
                        continue;
                    }
                    var result = await _musicSender.Send(musicRequest);
                    if (result.Success)
                    {
                        request.Completed = DateTime.UtcNow;
                        await _requestQueue.SaveChangesAsync();
                    }
                }
            }
        }
    }
}