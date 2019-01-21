using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class PlexAvailabilityRule : BaseSearchRule, IRules<SearchViewModel>
    {
        public PlexAvailabilityRule(IPlexContentRepository repo)
        {
            PlexContentRepository = repo;
        }

        private IPlexContentRepository PlexContentRepository { get; }

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            PlexServerContent item = null;
            var useImdb = false;
            var useTheMovieDb = false;
            var useTvDb = false;
            if (obj.ImdbId.HasValue())
            {
                item = await PlexContentRepository.Get(obj.ImdbId);
                if (item != null)
                {
                    useImdb = true;
                }
            }
            if (item == null)
            {
                if (obj.TheMovieDbId.HasValue())
                {
                    item = await PlexContentRepository.Get(obj.TheMovieDbId);
                    if (item != null)
                    {
                        useTheMovieDb = true;
                    }
                }

                if (item == null)
                {
                    if (obj.TheTvDbId.HasValue())
                    {
                        item = await PlexContentRepository.Get(obj.TheTvDbId);
                        if (item != null)
                        {
                            useTvDb = true;
                        }
                    }
                }
            }

            if (item != null)
            {
                obj.Available = true;
                obj.PlexUrl = item.Url;
                obj.Quality = item.Quality;
                
                if (obj.Type == RequestType.TvShow)
                {
                    var search = (SearchTvShowViewModel)obj;
                    // Let's go through the episodes now
                    if (search.SeasonRequests.Any())
                    {
                        var allEpisodes = PlexContentRepository.GetAllEpisodes();
                        foreach (var season in search.SeasonRequests)
                        {
                            foreach (var episode in season.Episodes)
                            {
                                await AvailabilityRuleHelper.SingleEpisodeCheck(useImdb, allEpisodes, episode, season, item, useTheMovieDb, useTvDb);
                            }
                        }

                        AvailabilityRuleHelper.CheckForUnairedEpisodes(search);
                    }
                }
            }
            return Success();
        }

        
    }
}