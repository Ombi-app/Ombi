using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Engine;
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
                var currentRequest = await tvRequests.FirstOrDefaultAsync(x => x.ParentRequest.ExternalProviderId == tv.Id); // the Id on the child is the TheMovieDb at this point
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

                    var episodesToRemove = new List<EpisodeRequests>();
                    foreach (var e in season.Episodes)
                    {
                        var existingEpRequest = currentSeasonRequest.Episodes.FirstOrDefault(x => x.EpisodeNumber == e.EpisodeNumber);
                        if (existingEpRequest != null)
                        {
                            episodesToRemove.Add(e);
                        }
                    }

                    episodesToRemove.ForEach(x =>
                    {
                        season.Episodes.Remove(x);
                    });
                }

                var anyEpisodes = tv.SeasonRequests.SelectMany(x => x.Episodes).Any();

                if (!anyEpisodes)
                {
                    return Fail(ErrorCode.EpisodesAlreadyRequested, $"We already have episodes requested from series {tv.Title}");
                }

            }
            return Success();
        }
    }
}