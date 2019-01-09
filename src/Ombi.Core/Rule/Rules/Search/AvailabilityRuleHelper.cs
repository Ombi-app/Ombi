using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Ombi.Core.Models.Search;
using Ombi.Store.Entities;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Rule.Rules.Search
{
    public static class AvailabilityRuleHelper
    {
        public static void CheckForUnairedEpisodes(SearchTvShowViewModel search)
        {
            if (search.SeasonRequests.All(x => x.Episodes.All(e => e.Available)))
            {
                search.FullyAvailable = true;
            }
            else
            {
                var airedButNotAvailable = search.SeasonRequests.Any(x =>
                    x.Episodes.Any(c => !c.Available && c.AirDate <= DateTime.Now.Date && c.AirDate != DateTime.MinValue));
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

        public static async Task SingleEpisodeCheck(bool useImdb, IQueryable<PlexEpisode> allEpisodes, EpisodeRequests episode,
            SeasonRequests season, PlexServerContent item, bool useTheMovieDb, bool useTvDb)
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
        public static async Task SingleEpisodeCheck(bool useImdb, IQueryable<EmbyEpisode> allEpisodes, EpisodeRequests episode,
            SeasonRequests season, EmbyContent item, bool useTheMovieDb, bool useTvDb)
        {
            EmbyEpisode epExists = null;
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
}