using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Models;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities;

namespace Ombi.Core.Engine
{
    public interface IVoteEngine
    {
        Task<VoteEngineResult> DownVote(int requestId, RequestType requestType);
        Task<Votes> GetVoteForUser(int requestId, string userId);
        IQueryable<Votes> GetVotes(int requestId, RequestType requestType);
        Task RemoveCurrentVote(Votes currentVote);
        Task<VoteEngineResult> UpVote(int requestId, RequestType requestType);
        Task<List<VoteViewModel>> GetMovieViewModel();
    }
}