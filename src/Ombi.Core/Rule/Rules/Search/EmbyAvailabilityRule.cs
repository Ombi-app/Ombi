using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class EmbyAvailabilityRule : BaseSearchRule, IRules<SearchViewModel>
    {
        public EmbyAvailabilityRule(IEmbyContentRepository repo, ISettingsService<EmbySettings> s)
        {
            EmbyContentRepository = repo;
            EmbySettings = s;
        }

        private IEmbyContentRepository EmbyContentRepository { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            EmbyContent item = null;
            var useImdb = false;
            var useTheMovieDb = false;
            var useTvDb = false;

            if (obj.ImdbId.HasValue())
            {
                item = await EmbyContentRepository.GetByImdbId(obj.ImdbId);
                if (item != null)
                {
                    useImdb = true;
                }
            }
            if (item == null)
            {
                if (obj.TheMovieDbId.HasValue())
                {
                    item = await EmbyContentRepository.GetByTheMovieDbId(obj.TheMovieDbId);
                    if (item != null)
                    {
                        useTheMovieDb = true;
                    }
                }

                if (item == null)
                {
                    if (obj.TheTvDbId.HasValue())
                    {
                        item = await EmbyContentRepository.GetByTvDbId(obj.TheTvDbId);
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
                var s = await EmbySettings.GetSettingsAsync();
                if (s.Enable)
                {
                    var server = s.Servers.FirstOrDefault(x => x.ServerHostname != null);
                    if ((server?.ServerHostname ?? string.Empty).HasValue())
                    {
                        obj.EmbyUrl = EmbyHelper.GetEmbyMediaUrl(item.EmbyId, server?.ServerId, server?.ServerHostname);
                    }
                    else
                    {
                        obj.EmbyUrl = EmbyHelper.GetEmbyMediaUrl(item.EmbyId, server?.ServerId, null);
                    }
                }

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
                                await AvailabilityRuleHelper.SingleEpisodeCheck(useImdb, allEpisodes, episode, season, item, useTheMovieDb, useTvDb);
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
