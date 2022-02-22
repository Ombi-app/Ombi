using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Services;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class JellyfinAvailabilityRule : BaseSearchRule, IRules<SearchViewModel>
    {
        private readonly IFeatureService _featureService;

        public JellyfinAvailabilityRule(IJellyfinContentRepository repo, ILogger<JellyfinAvailabilityRule> log, IFeatureService featureService)
        {
            JellyfinContentRepository = repo;
            Log = log;
            _featureService = featureService;
        }

        private IJellyfinContentRepository JellyfinContentRepository { get; }
        private ILogger Log { get; }

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            JellyfinContent item = null;
            var useImdb = false;
            var useTheMovieDb = false;
            var useTvDb = false;
            var useId = false;

            if (obj.ImdbId.HasValue())
            {
                item = await JellyfinContentRepository.GetByImdbId(obj.ImdbId);
                if (item != null)
                {
                    useImdb = true;
                }
            }
            if (item == null)
            {
                if (obj.Id > 0)
                {
                    item = await JellyfinContentRepository.GetByTheMovieDbId(obj.Id.ToString());
                    if (item != null)
                    {
                        useId = true;
                    }
                }
                if (obj.TheMovieDbId.HasValue())
                {
                    item = await JellyfinContentRepository.GetByTheMovieDbId(obj.TheMovieDbId);
                    if (item != null)
                    {
                        useTheMovieDb = true;
                    }
                }

                if (item == null)
                {
                    if (obj.TheTvDbId.HasValue())
                    {
                        item = await JellyfinContentRepository.GetByTvDbId(obj.TheTvDbId);
                        if (item != null)
                        {
                            useTvDb = true;
                        }
                    }
                }
            }
            
            if (item != null)
            {
                if (useId)
                {
                    obj.TheMovieDbId = obj.Id.ToString();
                    useTheMovieDb = true;
                }
                if (obj is SearchMovieViewModel movie)
                {
                    var is4kEnabled = await _featureService.FeatureEnabled(FeatureNames.Movie4KRequests);
                    if (item.Has4K && is4kEnabled)
                    {
                        movie.Available4K = true;
                        obj.JellyfinUrl = item.Url;
                    }
                    else
                    {
                        obj.Available = true;
                        obj.EmbyUrl = item.Url;
                        obj.Quality = item.Quality;
                    }
                }
                else
                {
                    obj.Available = true;
                    obj.JellyfinUrl = item.Url;
                }

                if (obj.Type == RequestType.TvShow)
                {
                    var search = (SearchTvShowViewModel)obj;
                    // Let's go through the episodes now
                    if (search.SeasonRequests.Any())
                    {
                        var allEpisodes = JellyfinContentRepository.GetAllEpisodes().Include(x => x.Series);
                        foreach (var season in search.SeasonRequests)
                        {
                            foreach (var episode in season.Episodes)
                            {
                                await AvailabilityRuleHelper.SingleEpisodeCheck(useImdb, allEpisodes, episode, season, item, useTheMovieDb, useTvDb, Log);
                            }
                        }
                    }

                    AvailabilityRuleHelper.CheckForUnairedEpisodes(search);
                }
            }
            return Success();
        }
    }
}
