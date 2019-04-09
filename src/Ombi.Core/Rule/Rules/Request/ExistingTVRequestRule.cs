using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Rule.Rules.Request
{
    public class ExistingTvRequestRule : BaseRequestRule, IRules<BaseRequest>
    {
        public ExistingTvRequestRule(ITvRequestRepository rv)
        {
            Tv = rv;
        }

        private ITvRequestRepository Tv { get; }

        /// <summary>
        /// We check if the request exists, if it does then we don't want to re-request it.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public async Task<RuleResult> Execute(BaseRequest obj)
        {
            if (obj.RequestType == RequestType.TvShow)
            {
                var tv = (ChildRequests) obj;
                var tvRequests = Tv.GetChild();
                var currentRequest = await tvRequests.FirstOrDefaultAsync(x => x.ParentRequest.TvDbId == tv.Id); // the Id on the child is the tvdbid at this point
                if (currentRequest == null)
                {
                    return Success();
                }
                foreach (var season in tv.SeasonRequests)
                {
                    var currentSeasonRequest =
                        currentRequest.SeasonRequests.FirstOrDefault(x => x.SeasonNumber == season.SeasonNumber);
                    if (currentSeasonRequest == null)
                    {
                        continue;
                    }
                    foreach (var e in season.Episodes)
                    {
                        var hasEpisode = currentSeasonRequest.Episodes.Any(x => x.EpisodeNumber == e.EpisodeNumber);
                        if (hasEpisode)
                        {
                            return Fail($"We already have episodes requested from series {tv.Title}");
                        }
                    }
                }
            }
            return Success();
        }
    }
}