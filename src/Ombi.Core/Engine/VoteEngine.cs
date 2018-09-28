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
using Ombi.Schedule.Jobs.Ombi;
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
            foreach (var r in movieRequests)
            {
                // Make model
                var votes = GetVotes(r.Id, RequestType.Movie);
                var upVotes = await votes.Where(x => x.VoteType == VoteType.Upvote).CountAsync();
                var downVotes = await votes.Where(x => x.VoteType == VoteType.Downvote).CountAsync();
                vm.Add(new VoteViewModel
                {
                    Upvotes = upVotes,
                    Downvotes = downVotes,
                    RequestId = r.Id,
                    RequestType = RequestType.Movie,
                    Title = r.Title,
                    Image = $"https://image.tmdb.org/t/p/w500/{r.PosterPath}",
                    Background = $"https://image.tmdb.org/t/p/w1280{r.Background}",
                    Description = r.Overview
                });
            }

            foreach (var r in await musicRequestsTask)
            {
                // Make model
                var votes = GetVotes(r.Id, RequestType.Album);
                var upVotes = await votes.Where(x => x.VoteType == VoteType.Upvote).CountAsync();
                var downVotes = await votes.Where(x => x.VoteType == VoteType.Downvote).CountAsync();
                vm.Add(new VoteViewModel
                {
                    Upvotes = upVotes,
                    Downvotes = downVotes,
                    RequestId = r.Id,
                    RequestType = RequestType.Album,
                    Title = r.Title,
                    Image = r.Cover,
                    Background = r.Cover,
                    Description = r.ArtistName
                });
            }

            foreach (var r in await tvRequestsTask)
            {
                // Make model
                var votes = GetVotes(r.Id, RequestType.TvShow);
                var upVotes = await votes.Where(x => x.VoteType == VoteType.Upvote).CountAsync();
                var downVotes = await votes.Where(x => x.VoteType == VoteType.Downvote).CountAsync();

                var finalsb = new StringBuilder();
                foreach (var childRequests in r.ChildRequests)
                {
                    foreach (var epInformation in childRequests.SeasonRequests.OrderBy(x => x.SeasonNumber))
                    {
                        var orderedEpisodes = epInformation.Episodes.OrderBy(x => x.EpisodeNumber).ToList();
                        var episodeString = NewsletterJob.BuildEpisodeList(orderedEpisodes.Select(x => x.EpisodeNumber));
                        finalsb.Append($"Season: {epInformation.SeasonNumber} - Episodes: {episodeString}");
                        finalsb.Append("<br />");
                    }
                }
                vm.Add(new VoteViewModel
                {
                    Upvotes = upVotes,
                    Downvotes = downVotes,
                    RequestId = r.Id,
                    RequestType = RequestType.TvShow,
                    Title = r.Title,
                    Image = r.PosterPath,
                    Background = r.Background,
                    Description = finalsb.ToString()
                });
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