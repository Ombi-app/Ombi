using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core.Models.Search;
using Ombi.Store.Entities;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Rule.Rules.Search
{
    public static class AvailabilityRuleHelper
    {
        public static void CheckForUnairedEpisodes(SearchTvShowViewModel search)
        {
            foreach (var season in search.SeasonRequests.ToList())
            {
                // If we have all the episodes for this season, then this season is available
                if (season.Episodes.All(x => x.Available))
                {
                    season.SeasonAvailable = true;
                }
            }
            if (search.SeasonRequests.All(x => x.Episodes.All(e => e.Available)))
            {
                search.FullyAvailable = true;
            }
            else if (search.SeasonRequests.Any(x => x.Episodes.Any(e => e.Available)))
            {
                search.PartlyAvailable = true;
            }

            if (!search.FullyAvailable)
            {
                var airedButNotAvailable = search.SeasonRequests.Any(x =>
                    x.Episodes.Any(c => !c.Available && c.AirDate <= DateTime.Now.Date && c.AirDate != DateTime.MinValue));
                if (!airedButNotAvailable)
                {
                    var unairedEpisodes = search.SeasonRequests.Any(x =>
                        x.Episodes.Any(c => !c.Available && c.AirDate > DateTime.Now.Date || c.AirDate != DateTime.MinValue));
                    if (unairedEpisodes)
                    {
                        search.FullyAvailable = true;
                    }
                }
            }
        }

        public static async Task SingleEpisodeCheck(bool useImdb, IQueryable<IMediaServerEpisode> allEpisodes, EpisodeRequests episode,
            SeasonRequests season, IMediaServerContent item, bool useTheMovieDb, bool useTvDb, ILogger log)
        {
            IMediaServerEpisode epExists = null;
            try
            {

                if (useImdb)
                {
                    epExists = await allEpisodes.FirstOrDefaultAsync(x =>
                        x.EpisodeNumber == episode.EpisodeNumber && x.SeasonNumber == season.SeasonNumber &&
                        x.Series.ImdbId == item.ImdbId);
                }

                if (useTheMovieDb)
                {
                    epExists = await allEpisodes.FirstOrDefaultAsync(x =>
                        x.EpisodeNumber == episode.EpisodeNumber && x.SeasonNumber == season.SeasonNumber &&
                        x.Series.TheMovieDbId == item.TheMovieDbId);
                }

                if (useTvDb)
                {
                    epExists = await allEpisodes.FirstOrDefaultAsync(x =>
                        x.EpisodeNumber == episode.EpisodeNumber && x.SeasonNumber == season.SeasonNumber &&
                        x.Series.TvDbId == item.TvDbId);
                }
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception thrown when attempting to check if something is available");
            }

            if (epExists != null)
            {
                episode.Available = true;
            }
        }
    }
}
