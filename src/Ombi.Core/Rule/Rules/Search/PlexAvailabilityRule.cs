using System;
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
                                PlexEpisode epExists = null;
                                if (useImdb)
                                {
                                    epExists = await allEpisodes.FirstOrDefaultAsync(x =>
                                        x.EpisodeNumber == episode.EpisodeNumber && x.SeasonNumber == season.SeasonNumber &&
                                        x.Series.ImdbId == item.ImdbId.ToString());
                                }
                                if (useTheMovieDb)
                                {
                                    epExists = await allEpisodes.FirstOrDefaultAsync(x =>
                                        x.EpisodeNumber == episode.EpisodeNumber && x.SeasonNumber == season.SeasonNumber &&
                                        x.Series.TheMovieDbId == item.TheMovieDbId.ToString());
                                }
                                if (useTvDb)
                                {
                                    epExists = await allEpisodes.FirstOrDefaultAsync(x =>
                                        x.EpisodeNumber == episode.EpisodeNumber && x.SeasonNumber == season.SeasonNumber &&
                                        x.Series.TvDbId == item.TvDbId.ToString());
                                }
                                
                                if (epExists != null)
                                {
                                    episode.Available = true;
                                }
                            }
                        }

                        if (search.SeasonRequests.All(x => x.Episodes.All(e => e.Available)))
                        {
                            search.FullyAvailable = true;
                        }
                        else
                        {
                            var airedButNotAvailable = search.SeasonRequests.Any(x =>
                                x.Episodes.Any(c => !c.Available && c.AirDate <= DateTime.Now.Date));
                            if (!airedButNotAvailable)
                            {
                                var unairedEpisodes = search.SeasonRequests.Any(x =>
                                    x.Episodes.Any(c => !c.Available && c.AirDate > DateTime.Now.Date));
                                if (unairedEpisodes)
                                {
                                    search.FullyAvailable = true;
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