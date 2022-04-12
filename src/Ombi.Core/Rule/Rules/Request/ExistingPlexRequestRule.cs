using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Engine;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

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
                
                var tvContent = _plexContent.GetAll().Include(x => x.Episodes).Where(x => x.Type == MediaType.Series);
                // We need to do a check on the TVDBId
                var anyMovieDbMatches = await tvContent.FirstOrDefaultAsync(x => x.TheMovieDbId.Length > 0 && x.TheMovieDbId == tvRequest.Id.ToString()); 
                if (anyMovieDbMatches == null)
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
                return CheckExistingContent(tvRequest, anyMovieDbMatches);
            }
            if (obj.RequestType == RequestType.Movie)
            {
                var movie = (MovieRequests)obj;
                var exists = _plexContent.GetAll().Where(x => x.Type == MediaType.Movie).Any(x => x.TheMovieDbId == movie.Id.ToString() || x.TheMovieDbId == movie.TheMovieDbId.ToString());
                if (exists)
                {
                    return Fail(ErrorCode.AlreadyRequested, "This movie is already available." );
                }
            }
            return Success();
        }


        private RuleResult CheckExistingContent(ChildRequests child, PlexServerContent content)
        {
            foreach (var season in child.SeasonRequests)
            {
                var episodesToRemove = new List<EpisodeRequests>();
                var currentSeasonRequest =
                    content.Episodes.Where(x => x.SeasonNumber == season.SeasonNumber).ToList();
                if (!currentSeasonRequest.Any())
                {
                    continue;
                }
                foreach (var e in season.Episodes)
                {
                    var existingEpRequest = currentSeasonRequest.FirstOrDefault(x => x.EpisodeNumber == e.EpisodeNumber);
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

            var anyEpisodes = child.SeasonRequests.SelectMany(x => x.Episodes).Any();

            if (!anyEpisodes)
            {
                return Fail(ErrorCode.EpisodesAlreadyRequested, $"We already have episodes requested from series {child.Title}");
            }

            return Success();
        }
    }
}