using Microsoft.Extensions.Logging;
using Ombi.Api.Sonarr;
using Ombi.Api.Sonarr.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Core
{
    public class TvSender : ITvSender
    {
        public TvSender(ISonarrApi sonarrApi, ILogger<TvSender> log, ISettingsService<SonarrSettings> settings)
        {
            SonarrApi = sonarrApi;
            Logger = log;
            Settings = settings;
        }

        private ISonarrApi SonarrApi { get; }
        private ILogger<TvSender> Logger { get; }
        private ISettingsService<SonarrSettings> Settings { get; }

        /// <summary>
        /// Send the request to Sonarr to process
        /// </summary>
        /// <param name="s"></param>
        /// <param name="model"></param>
        /// <param name="qualityId">This is for any qualities overriden from the UI</param>
        /// <returns></returns>
        public async Task<NewSeries> SendToSonarr(ChildRequests model, string qualityId = null)
        {
            var s = await Settings.GetSettingsAsync();
            var qualityProfile = 0;
            if (!string.IsNullOrEmpty(qualityId)) // try to parse the passed in quality, otherwise use the settings default quality
            {
                int.TryParse(qualityId, out qualityProfile);
            }

            if (qualityProfile <= 0)
            {
                int.TryParse(s.QualityProfile, out qualityProfile);
            }

            // Get the root path from the rootfolder selected.
            // For some reason, if we haven't got one use the first root folder in Sonarr
            // TODO make this overrideable via the UI
            var rootFolderPath = await GetSonarrRootPath(model.ParentRequest.RootFolder ?? 0, s);
            try
            {
                // Does the series actually exist?
                var allSeries = await SonarrApi.GetSeries(s.ApiKey, s.FullUri);
                var existingSeries = allSeries.FirstOrDefault(x => x.tvdbId == model.ParentRequest.TvDbId);

                if (existingSeries == null)
                {
                    // Time to add a new one
                    var newSeries = new NewSeries
                    {
                        title = model.ParentRequest.Title,
                        imdbId = model.ParentRequest.ImdbId,
                        tvdbId = model.ParentRequest.TvDbId,
                        cleanTitle = model.ParentRequest.Title,
                        monitored = true,
                        seasonFolder = s.SeasonFolders,
                        rootFolderPath = rootFolderPath,
                        qualityProfileId = qualityProfile,
                        titleSlug = model.ParentRequest.Title,
                        addOptions = new AddOptions
                        {
                            ignoreEpisodesWithFiles = true, // There shouldn't be any episodes with files, this is a new season
                            ignoreEpisodesWithoutFiles = true, // We want all missing
                            searchForMissingEpisodes = false // we want dont want to search yet. We want to make sure everything is unmonitored/monitored correctly.
                        }
                    };

                    // Montitor the correct seasons,
                    // If we have that season in the model then it's monitored!
                    var seasonsToAdd = new List<Season>();
                    for (int i = 1; i < model.ParentRequest.TotalSeasons + 1; i++)
                    {
                        var season = new Season
                        {
                            seasonNumber = i,
                            monitored = model.SeasonRequests.Any(x => x.SeasonNumber == i)
                        };
                        seasonsToAdd.Add(season);
                    }
                    newSeries.seasons = seasonsToAdd;
                    var result = await SonarrApi.AddSeries(newSeries, s.ApiKey, s.FullUri);

                    // Ok, now let's sort out the episodes.
                    var sonarrEpisodes = await SonarrApi.GetEpisodes(result.id, s.ApiKey, s.FullUri);
                    while (sonarrEpisodes.Count() == 0)
                    {
                        // It could be that the series metadata is not ready yet. So wait
                        sonarrEpisodes = await SonarrApi.GetEpisodes(result.id, s.ApiKey, s.FullUri);
                        await Task.Delay(300);
                    }

                    var episodesToUpdate = new List<Episode>();
                    foreach (var req in model.SeasonRequests)
                    {
                        foreach (var ep in req.Episodes)
                        {
                            var sonarrEp = sonarrEpisodes.FirstOrDefault(x => x.episodeNumber == ep.EpisodeNumber && x.seasonNumber == ep.Season.SeasonNumber);
                            if (sonarrEp != null)
                            {
                                sonarrEp.monitored = true;
                                episodesToUpdate.Add(sonarrEp);
                            }
                        }
                    }

                    // Now update the episodes that need updating
                    foreach (var epToUpdate in episodesToUpdate)
                    {
                        await SonarrApi.UpdateEpisode(epToUpdate, s.ApiKey, s.FullUri);
                    }

                    // TODO possibly update the season as it might be unmonitored due to the clash with the AddOptions                    

                    if (!s.AddOnly)
                    {
                        foreach (var season in model.SeasonRequests)
                        {
                            var sonarrSeason = sonarrEpisodes.Where(x => x.seasonNumber == season.SeasonNumber);
                            var sonarrEpCount = sonarrSeason.Count();
                            var ourRequestCount = season.Episodes.Count();

                            if (sonarrEpCount == ourRequestCount)
                            {
                                // We have the same amount of requests as all of the episodes in the season.
                                // Do a season search
                                await SonarrApi.SeasonSearch(result.id, season.SeasonNumber, s.ApiKey, s.FullUri);
                            }
                            else
                            {
                                // There is a miss-match, let's search the episodes indiviaully 
                                await SonarrApi.EpisodeSearch(episodesToUpdate.Select(x => x.id).ToArray(), s.ApiKey, s.FullUri);
                            }
                        }
                    }

                    return result;
                }
                else
                {

                    var sonarrEpisodes = await SonarrApi.GetEpisodes(existingSeries.id, s.ApiKey, s.FullUri);

                    var episodesToUpdate = new List<Episode>();
                    foreach (var req in model.SeasonRequests)
                    {
                        foreach (var ep in req.Episodes)
                        {
                            var sonarrEp = sonarrEpisodes.FirstOrDefault(x => x.episodeNumber == ep.EpisodeNumber && x.seasonNumber == ep.Season.SeasonNumber);
                            if (sonarrEp != null)
                            {
                                sonarrEp.monitored = true;
                                episodesToUpdate.Add(sonarrEp);
                            }
                        }
                    }

                    // Now update the episodes that need updating
                    foreach (var epToUpdate in episodesToUpdate)
                    {
                        await SonarrApi.UpdateEpisode(epToUpdate, s.ApiKey, s.FullUri);
                    }

                    // TODO possibly update the season as it might be unmonitored due to the clash with the AddOptions                    

                    if (!s.AddOnly)
                    {
                        foreach (var season in model.SeasonRequests)
                        {
                            var sonarrSeason = sonarrEpisodes.Where(x => x.seasonNumber == season.SeasonNumber);
                            var sonarrEpCount = sonarrSeason.Count();
                            var ourRequestCount = season.Episodes.Count();

                            if (sonarrEpCount == ourRequestCount)
                            {
                                // We have the same amount of requests as all of the episodes in the season.
                                // Do a season search
                                await SonarrApi.SeasonSearch(existingSeries.id, season.SeasonNumber, s.ApiKey, s.FullUri);
                            }
                            else
                            {
                                // There is a miss-match, let's search the episodes indiviaully 
                                await SonarrApi.EpisodeSearch(episodesToUpdate.Select(x => x.id).ToArray(), s.ApiKey, s.FullUri);
                            }
                        }
                    }
                }

                return new NewSeries
                {
                    id = existingSeries.id,
                    seasons = existingSeries.seasons.ToList(),
                    cleanTitle = existingSeries.cleanTitle,
                    title = existingSeries.title,
                    tvdbId = existingSeries.tvdbId
                };
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.SonarrSender, e, "Exception thrown when attempting to send series over to Sonarr");
                throw;
            }
        }

        private async Task<string> GetSonarrRootPath(int pathId, SonarrSettings sonarrSettings)
        {
            var rootFoldersResult = await SonarrApi.GetRootFolders(sonarrSettings.ApiKey, sonarrSettings.FullUri);

            if (pathId == 0)
            {
                return rootFoldersResult.FirstOrDefault().path;
            }

            foreach (var r in rootFoldersResult.Where(r => r.id == pathId))
            {
                return r.path;
            }
            return string.Empty;
        }
    }
}