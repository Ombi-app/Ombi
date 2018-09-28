using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Engine
{
    public class VoteEngine : BaseEngine, IVoteEngine
    {
        public VoteEngine(IRepository<Votes> votes, IPrincipal user, OmbiUserManager um, IRuleEvaluator r, ISettingsService<VoteSettings> voteSettings,
            IMusicRequestEngine musicRequestEngine, ITvRequestEngine tvRequestEngine, IMovieRequestEngine movieRequestEngine) : base(user, um, r)
        {
            _voteRepository = votes;
            _voteSettings = voteSettings;
            _movieRequestEngine = movieRequestEngine;
            _musicRequestEngine = musicRequestEngine;
            _tvRequestEngine = tvRequestEngine;
        }

        private readonly IRepository<Votes> _voteRepository;
        private readonly ISettingsService<VoteSettings> _voteSettings;
        private readonly IMusicRequestEngine _musicRequestEngine;
        private readonly ITvRequestEngine _tvRequestEngine;
        private readonly IMovieRequestEngine _movieRequestEngine;

        public async Task GetMovieViewModel()
        {
            var requests = await _movieRequestEngine.GetRequests();
            foreach (var r in requests)
            {
                // Make model
                var votes = GetVotes(r.Id, RequestType.Movie);
            }
        }

        public IQueryable<Votes> GetVotes(int requestId, RequestType requestType)
        {
            return _voteRepository.GetAll().Where(x => x.RequestType == requestType && requestId == x.RequestId);
        }

        public Task<Votes> GetVoteForUser(int requestId, string userId)
        {
            return _voteRepository.GetAll().FirstOrDefaultAsync(x => x.RequestId == requestId && x.UserId == userId);
        }

        public async Task<VoteEngineResult> UpVote(int requestId, RequestType requestType)
        {
            // How many votes does this have?!
            var currentVotes = GetVotes(requestId, requestType);
            var voteSettings = await _voteSettings.GetSettingsAsync();

            // Does this user have a downvote? If so we should revert it and make it an upvote
            var user = await GetUser();

            var currentVote = await GetVoteForUser(requestId, user.Id);
            if (currentVote != null && currentVote.VoteType == VoteType.Upvote)
            {
                return new VoteEngineResult { ErrorMessage = "You have already voted!" };
            }
            await RemoveCurrentVote(currentVote);

            await _voteRepository.Add(new Votes
            {
                Date = DateTime.UtcNow,
                RequestId = requestId,
                RequestType = requestType,
                UserId = user.Id,
                VoteType = VoteType.Upvote
            });

            var upVotes = await currentVotes.Where(x => x.VoteType == VoteType.Upvote).CountAsync();
            var downVotes = -(await currentVotes.Where(x => x.VoteType == VoteType.Downvote).CountAsync());

            var totalVotes = upVotes + downVotes;
            RequestEngineResult result = null;
            switch (requestType)
            {
                case RequestType.TvShow:
                    if (totalVotes >= voteSettings.TvShowVoteMax)
                    {
                        result = await _tvRequestEngine.ApproveChildRequest(requestId);
                    }
                    break;
                case RequestType.Movie:
                    if (totalVotes >= voteSettings.MovieVoteMax)
                    {
                        result = await _movieRequestEngine.ApproveMovieById(requestId);
                    }
                    break;
                case RequestType.Album:
                    if (totalVotes >= voteSettings.MusicVoteMax)
                    {
                        result = await _musicRequestEngine.ApproveAlbumById(requestId);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(requestType), requestType, null);
            }

            if (result != null && !result.Result)
            {
                return new VoteEngineResult
                {
                    ErrorMessage = "Voted succesfully but could not approve movie!"
                };
            }

            return new VoteEngineResult
            {
                Result = true
            };
        }

        public async Task<VoteEngineResult> DownVote(int requestId, RequestType requestType)
        {
            var user = await GetUser();
            var currentVote = await GetVoteForUser(requestId, user.Id);
            if (currentVote != null && currentVote.VoteType == VoteType.Upvote)
            {
                return new VoteEngineResult { ErrorMessage = "You have already voted!" };
            }
            await RemoveCurrentVote(currentVote);

            await _voteRepository.Add(new Votes
            {
                Date = DateTime.UtcNow,
                RequestId = requestId,
                RequestType = requestType,
                UserId = user.Id,
                VoteType = VoteType.Downvote
            });

            return new VoteEngineResult
            {
                Result = true
            };
        }

        public async Task RemoveCurrentVote(Votes currentVote)
        {
            if (currentVote != null)
            {
                await _voteRepository.Delete(currentVote);
            }
        }
    }
}