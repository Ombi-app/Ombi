using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
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
            var item = await PlexContentRepository.Get(obj.CustomId);
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
                                var epExists = await allEpisodes.FirstOrDefaultAsync(x =>
                                    x.EpisodeNumber == episode.EpisodeNumber && x.SeasonNumber == season.SeasonNumber &&
                                    x.Series.ProviderId == item.ProviderId.ToString());
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