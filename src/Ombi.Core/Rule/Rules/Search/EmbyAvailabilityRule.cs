using System;
using System.Linq;
using System.Linq.Expressions;
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
                item = await EmbyContentRepository.GetByImdbId(obj.ImdbId);
            }
            if (item == null)
            {
                if (obj.TheMovieDbId.HasValue())
                {
                    item = await EmbyContentRepository.GetByTheMovieDbId(obj.TheMovieDbId);
                }

                if (item == null)
                {
                    if (obj.TheTvDbId.HasValue())
                    {
                        item = await EmbyContentRepository.GetByTvDbId(obj.TheTvDbId);
                    }
                }
            }

            if (item != null)
            {
                obj.Available = true;
                obj.EmbyUrl = item.Url;

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

                                if (item.HasImdb)
                                {
                                    epExists = await allEpisodes.FirstOrDefaultAsync(e => e.EpisodeNumber == episode.EpisodeNumber && e.SeasonNumber == season.SeasonNumber
                                        && e.ImdbId == item.ImdbId);
                                }  if (item.HasTvDb && epExists == null)
                                {
                                    epExists = await allEpisodes.FirstOrDefaultAsync(e => e.EpisodeNumber == episode.EpisodeNumber && e.SeasonNumber == season.SeasonNumber
                                                                                         && e.Series.TvDbId == item.TvDbId);
                                }  if (item.HasTheMovieDb && epExists == null)
                                {
                                    epExists = await allEpisodes.FirstOrDefaultAsync(e => e.EpisodeNumber == episode.EpisodeNumber && e.SeasonNumber == season.SeasonNumber
                                                                                         && e.TheMovieDbId == item.TheMovieDbId);
                                }

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