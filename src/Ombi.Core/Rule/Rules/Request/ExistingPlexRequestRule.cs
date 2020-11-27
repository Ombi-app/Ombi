using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Request
{
    public class ExistingPlexRequestRule : BaseRequestRule, IRules<BaseRequest>
    {
        public ExistingPlexRequestRule(IPlexContentRepository rv)
        {
            _plexContent = rv;
        }

        private readonly IPlexContentRepository _plexContent;

        /// <summary>
        /// We check if the request exists, if it does then we don't want to re-request it.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public async Task<RuleResult> Execute(BaseRequest obj)
        {
            if (obj.RequestType == RequestType.TvShow)
            {
                var tvRequest = (ChildRequests) obj;
                
                var tvContent = _plexContent.GetAll().Include(x => x.Episodes).Where(x => x.Type == PlexMediaTypeEntity.Show);
                // We need to do a check on the TVDBId
                var anyTvDbMatches = await tvContent.FirstOrDefaultAsync(x => x.TvDbId.Length > 0 && x.TvDbId == tvRequest.Id.ToString()); // the Id on the child is the tvdbid at this point
                if (anyTvDbMatches == null)
                {
                    // So we do not have a TVDB Id, that really sucks.
                    // Let's try and match on the title and year of the show
                    var titleAndYearMatch = await tvContent.FirstOrDefaultAsync(x =>
                        x.Title == tvRequest.Title
                        && x.ReleaseYear == tvRequest.ReleaseYear.Year.ToString());
                    if (titleAndYearMatch != null)
                    {
                        // We have a match! Surprise Motherfucker
                        return CheckExistingContent(tvRequest, titleAndYearMatch);
                    }

                    // We do not have this
                    return Success();
                }
                // looks like we have a match on the TVDbID
                return CheckExistingContent(tvRequest, anyTvDbMatches);
            }
            return Success();
        }


        private RuleResult CheckExistingContent(ChildRequests child, PlexServerContent content)
        {
            foreach (var season in child.SeasonRequests)
            {
                var currentSeasonRequest =
                    content.Episodes.Where(x => x.SeasonNumber == season.SeasonNumber).ToList();
                if (!currentSeasonRequest.Any())
                {
                    continue;
                }
                foreach (var e in season.Episodes)
                {
                    var hasEpisode = currentSeasonRequest.Any(x => x.EpisodeNumber == e.EpisodeNumber);
                    if (hasEpisode)
                    {
                        return Fail($"We already have episodes requested from series {child.Title}");
                    }
                }
            }

            return Success();
        }
    }
}