using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Helpers;
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
            EmbyContent item = null;
            if (obj.ImdbId.HasValue())
            {
                item = await EmbyContentRepository.Get(obj.ImdbId);
            }
            if (item == null)
            {
                if (obj.TheMovieDbId.HasValue())
                {
                    item = await EmbyContentRepository.Get(obj.TheMovieDbId);
                }

                if (item == null)
                {
                    if (obj.TheTvDbId.HasValue())
                    {
                        item = await EmbyContentRepository.Get(obj.TheTvDbId);
                    }
                }
            }

            if (item != null)
            {
                obj.Available = true;

                if (obj.Type == RequestType.TvShow)
                {
                    var search = (SearchTvShowViewModel)obj;
                    // Let's go through the episodes now
                    if (search.SeasonRequests.Any())
                    {
                        var allEpisodes = EmbyContentRepository.GetAllEpisodes().Include(x => x.Series);
                        foreach (var season in search.SeasonRequests)
                        {
                            foreach (var episode in season.Episodes)
                            {
                                EmbyEpisode epExists = null;

                                epExists = await allEpisodes.FirstOrDefaultAsync(x =>
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