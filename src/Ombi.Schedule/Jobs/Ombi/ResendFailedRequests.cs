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

        public async Task Execute(IJobExecutionContext job)
        {
            // Get all the failed ones!
            var failedRequests = _requestQueue.GetAll().Where(x => x.Completed == null);

            foreach (var request in failedRequests)
            {
                if (request.Type == RequestType.Movie)
                {
                    var movieRequest = await _movieRequestRepository.GetAll().FirstOrDefaultAsync(x => x.Id == request.RequestId);
                    var result = await _movieSender.Send(movieRequest);
                    if (result.Success)
                    {
                        request.Completed = DateTime.UtcNow;
                        await _requestQueue.SaveChangesAsync();
                    }
                }
                if (request.Type == RequestType.TvShow)
                {
                    var tvRequest = await _tvRequestRepository.GetChild().FirstOrDefaultAsync(x => x.Id == request.RequestId);
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