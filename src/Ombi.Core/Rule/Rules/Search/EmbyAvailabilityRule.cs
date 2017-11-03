using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class EmbyAvailabilityRule : BaseSearchRule, IRules<SearchViewModel>
    {
        public EmbyAvailabilityRule(IEmbyContentRepository repo)
        {
            EmbyContentRepository = repo;
        }

        private IEmbyContentRepository EmbyContentRepository { get; }

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            var item = await EmbyContentRepository.Get(obj.ImdbId);
            if (item != null)
            {
                obj.Available = true;

                if (obj.Type == RequestType.TvShow)
                {
                    var searchResult = (SearchTvShowViewModel)obj;
                    // Let's go through the episodes now
                    if (searchResult.SeasonRequests.Any())
                    {
                        var allEpisodes = EmbyContentRepository.GetAllEpisodes().Include(x => x.Series);
                        foreach (var season in searchResult.SeasonRequests)
                        {
                            foreach (var episode in season.Episodes)
                            {
                                var epExists = await allEpisodes.FirstOrDefaultAsync(x =>
                                    x.EpisodeNumber == episode.EpisodeNumber && x.SeasonNumber == season.SeasonNumber && item.ProviderId.ToString() == searchResult.Id.ToString());
                                if (epExists != null)
                                {
                                    episode.Available = true;
                                }
                            }
                        }
                    }
                }
            }
            return Success();
        }
    }
}