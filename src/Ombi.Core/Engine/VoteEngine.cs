using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Engine
{
    public class VoteEngine : BaseEngine
    {
        public VoteEngine(IRepository<Votes> votes, IPrincipal user, OmbiUserManager um, IRuleEvaluator r) : base(user, um, r)
        {
            _voteRepository = votes;
        }

        private readonly IRepository<Votes> _voteRepository;

        public async Task<Votes> GetVotesForMovie(int requestId)
        {
            return await _voteRepository.GetAll().FirstOrDefaultAsync(x => x.RequestType == RequestType.Movie && x.RequestId == requestId);
        }
        public IQueryable<Votes> GetVotesForMovie(IEnumerable<int> requestIds)
        {
            return _voteRepository.GetAll().Where(x => x.RequestType == RequestType.Movie && requestIds.Contains(x.RequestId));
        }

        public async Task<Votes> GetVotesForTv(int requestId)
        {
            return await _voteRepository.GetAll().FirstOrDefaultAsync(x => x.RequestType == RequestType.TvShow && x.RequestId == requestId);
        }

        public IQueryable<Votes> GetVotesForTv(IEnumerable<int> requestIds)
        {
            return _voteRepository.GetAll().Where(x => x.RequestType == RequestType.TvShow && requestIds.Contains(x.RequestId));
        }

        public async Task UpvoteMovie(int requestId)
        {
            var user = await GetUser();
            await _voteRepository.Add(new Votes
            {
                Date = DateTime.UtcNow,
                RequestId = requestId,
                RequestType = RequestType.Movie,
                UserId = user.Id,
                VoteType = VoteType.Upvote
            });
        }

        public async Task DownvoteMovie(int requestId)
        {
            var user = await GetUser();
            await _voteRepository.Add(new Votes
            {
                Date = DateTime.UtcNow,
                RequestId = requestId,
                RequestType = RequestType.Movie,
                UserId = user.Id,
                VoteType = VoteType.Downvote
            });
        }

        public async Task UpvoteTv(int requestId)
        {
            var user = await GetUser();
            await _voteRepository.Add(new Votes
            {
                Date = DateTime.UtcNow,
                RequestId = requestId,
                RequestType = RequestType.TvShow,
                UserId = user.Id,
                VoteType = VoteType.Upvote
            });
        }

        public async Task DownvoteTv(int requestId)
        {
            var user = await GetUser();
            await _voteRepository.Add(new Votes
            {
                Date = DateTime.UtcNow,
                RequestId = requestId,
                RequestType = RequestType.TvShow,
                UserId = user.Id,
                VoteType = VoteType.Downvote
            });
        }
    }
}