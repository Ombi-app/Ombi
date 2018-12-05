using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models;
using Ombi.Core.Models.UI;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
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

        public async Task<List<VoteViewModel>> GetMovieViewModel()
        {
            var vm = new List<VoteViewModel>();
            var movieRequests = await _movieRequestEngine.GetRequests();
            var tvRequestsTask = _tvRequestEngine.GetRequests();
            var musicRequestsTask = _musicRequestEngine.GetRequests();
            var user = await GetUser();
            foreach (var r in movieRequests)
            {
                if (r.Available || r.Approved || (r.Denied ?? false))
                {
                    continue;
                }
                // Make model
                var votes = GetVotes(r.Id, RequestType.Movie);
                var upVotes = await votes.Where(x => x.VoteType == VoteType.Upvote).CountAsync();
                var downVotes = await votes.Where(x => x.VoteType == VoteType.Downvote).CountAsync();
                var myVote = await votes.FirstOrDefaultAsync(x => x.UserId == user.Id && !x.Deleted);

                vm.Add(new VoteViewModel
                {
                    Upvotes = upVotes,
                    Downvotes = downVotes,
                    RequestId = r.Id,
                    RequestType = RequestType.Movie,
                    Title = r.Title,
                    Image = $"https://image.tmdb.org/t/p/w500/{r.PosterPath}",
                    Background = $"https://image.tmdb.org/t/p/w1280{r.Background}",
                    Description = r.Overview,
                    AlreadyVoted = myVote != null,
                    MyVote = myVote?.VoteType ?? VoteType.Downvote
                });
            }

            foreach (var r in await musicRequestsTask)
            {
                if (r.Available || r.Approved || (r.Denied ?? false))
                {
                    continue;
                }
                // Make model
                var votes = GetVotes(r.Id, RequestType.Album);
                var upVotes = await votes.Where(x => x.VoteType == VoteType.Upvote).CountAsync();
                var downVotes = await votes.Where(x => x.VoteType == VoteType.Downvote).CountAsync();
                var myVote = await votes.FirstOrDefaultAsync(x => x.UserId == user.Id && !x.Deleted);
                vm.Add(new VoteViewModel
                {
                    Upvotes = upVotes,
                    Downvotes = downVotes,
                    RequestId = r.Id,
                    RequestType = RequestType.Album,
                    Title = r.Title,
                    Image = r.Cover,
                    Background = r.Cover,
                    Description = r.ArtistName,
                    AlreadyVoted = myVote != null,
                    MyVote = myVote?.VoteType ?? VoteType.Downvote
                });
            }

            foreach (var r in await tvRequestsTask)
            {

                foreach (var childRequests in r.ChildRequests)
                {
                    var finalsb = new StringBuilder();
                    if (childRequests.Available || childRequests.Approved || (childRequests.Denied ?? false))
                    {
                        continue;
                    }
                    var votes = GetVotes(childRequests.Id, RequestType.TvShow);
                    // Make model
                    var upVotes = await votes.Where(x => x.VoteType == VoteType.Upvote).CountAsync();
                    var downVotes = await votes.Where(x => x.VoteType == VoteType.Downvote).CountAsync();
                    var myVote = await votes.FirstOrDefaultAsync(x => x.UserId == user.Id && !x.Deleted);
                    foreach (var epInformation in childRequests.SeasonRequests.OrderBy(x => x.SeasonNumber))
                    {
                        var orderedEpisodes = epInformation.Episodes.OrderBy(x => x.EpisodeNumber).ToList();
                        var episodeString = StringHelper.BuildEpisodeList(orderedEpisodes.Select(x => x.EpisodeNumber));
                        finalsb.Append($"Season: {epInformation.SeasonNumber} - Episodes: {episodeString}");
                        finalsb.Append("<br />");
                    }
                    vm.Add(new VoteViewModel
                    {
                        Upvotes = upVotes,
                        Downvotes = downVotes,
                        RequestId = childRequests.Id,
                        RequestType = RequestType.TvShow,
                        Title = r.Title,
                        Image = r.PosterPath,
                        Background = r.Background,
                        Description = finalsb.ToString(),
                        AlreadyVoted = myVote != null,
                        MyVote = myVote?.VoteType ?? VoteType.Downvote
                    });
                }
            }

            return vm;
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
            var voteSettings = await _voteSettings.GetSettingsAsync();
            if (!voteSettings.Enabled)
            {
                return new VoteEngineResult {Result = true};
            }
            // How many votes does this have?!
            var currentVotes = GetVotes(requestId, requestType);

            var user = await GetUser();

            // Does this user have a downvote? If so we should revert it and make it an upvote
            var currentVote = await GetVoteForUser(requestId, user.Id);
            if (currentVote != null && currentVote.VoteType == VoteType.Upvote)
            {
                return new VoteEngineResult { ErrorMessage = "You have already voted!" };
            }
            await RemoveCurrentVote(currentVote);
            await _movieRequestEngine.SubscribeToRequest(requestId, requestType);

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
                    ErrorMessage = "Voted succesfully but could not approve!"
                };
            }

            return new VoteEngineResult
            {
                Result = true
            };
        }

        public async Task<VoteEngineResult> DownVote(int requestId, RequestType requestType)
        {
            var voteSettings = await _voteSettings.GetSettingsAsync();
            if (!voteSettings.Enabled)
            {
                return new VoteEngineResult { Result = true };
            }
            var user = await GetUser();
            var currentVote = await GetVoteForUser(requestId, user.Id);
            if (currentVote != null && currentVote.VoteType == VoteType.Downvote)
            {
                return new VoteEngineResult { ErrorMessage = "You have already voted!" };
            }
            await RemoveCurrentVote(currentVote);

            await _movieRequestEngine.UnSubscribeRequest(requestId, requestType);

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