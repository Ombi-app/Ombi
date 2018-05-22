using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.DogNzb;
using Ombi.Api.DogNzb.Models;
using Ombi.Api.SickRage;
using Ombi.Api.SickRage.Models;
using Ombi.Api.Sonarr;
using Ombi.Api.Sonarr.Models;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Senders
{
    public class TvSender : ITvSender
    {
        public TvSender(ISonarrApi sonarrApi, ILogger<TvSender> log, ISettingsService<SonarrSettings> sonarrSettings,
            ISettingsService<DogNzbSettings> dog, IDogNzbApi dogApi, ISettingsService<SickRageSettings> srSettings,
            ISickRageApi srApi)
        {
            SonarrApi = sonarrApi;
            Logger = log;
            SonarrSettings = sonarrSettings;
            DogNzbSettings = dog;
            DogNzbApi = dogApi;
            SickRageSettings = srSettings;
            SickRageApi = srApi;
        }

        private ISonarrApi SonarrApi { get; }
        private IDogNzbApi DogNzbApi { get; }
        private ISickRageApi SickRageApi { get; }
        private ILogger<TvSender> Logger { get; }
        private ISettingsService<SonarrSettings> SonarrSettings { get; }
        private ISettingsService<DogNzbSettings> DogNzbSettings { get; }
        private ISettingsService<SickRageSettings> SickRageSettings { get; }

        public async Task<SenderResult> Send(ChildRequests model)
        {
            var sonarr = await SonarrSettings.GetSettingsAsync();
            if (sonarr.Enabled)
            {
                var result = await SendToSonarr(model);
                if (result != null)
                {
                    return new SenderResult
                    {
                        Sent = true,
                        Success = true
                    };
                }
            }
            var dog = await DogNzbSettings.GetSettingsAsync();
            if (dog.Enabled)
            {
                var result = await SendToDogNzb(model, dog);
                if (!result.Failure)
                {
                    return new SenderResult
                    {
                        Sent = true,
                        Success = true
                    };
                }
                return new SenderResult
                {
                    Message = result.ErrorMessage
                };
            }
            var sr = await SickRageSettings.GetSettingsAsync();
            if (sr.Enabled)
            {
                var result = await SendToSickRage(model, sr);
                if (result)
                {
                    return new SenderResult
                    {
                        Sent = true,
                        Success = true
                    };
                }
                return new SenderResult
                {
                    Message = "Could not send to SickRage!"
                };
            }
            return new SenderResult
            {
                Success = true
            };
        }

        private async Task<DogNzbAddResult> SendToDogNzb(ChildRequests model, DogNzbSettings settings)
        {
            var id = model.ParentRequest.TvDbId;
            return await DogNzbApi.AddTvShow(settings.ApiKey, id.ToString());
        }

        /// <summary>
        /// Send the request to Sonarr to process
        /// </summary>
        /// <param name="s"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<NewSeries> SendToSonarr(ChildRequests model)
        {
            var s = await SonarrSettings.GetSettingsAsync();
            if (!s.Enabled)
            {
                return null;
            }
            if (string.IsNullOrEmpty(s.ApiKey))
            {
                return null;
            }

            int qualityToUse;
            string rootFolderPath;

            if (model.SeriesType == SeriesType.Anime)
            {
                // Get the root path from the rootfolder selected.
                // For some reason, if we haven't got one use the first root folder in Sonarr
                // TODO make this overrideable via the UI
                rootFolderPath = await GetSonarrRootPath(model.ParentRequest.RootFolder ?? int.Parse(s.RootPathAnime), s);
                int.TryParse(s.QualityProfileAnime, out qualityToUse);
            }
            else
            {
                int.TryParse(s.QualityProfile, out qualityToUse);
            }

            if (model.ParentRequest.QualityOverride.HasValue)
            {
                // Get the root path from the rootfolder selected.
                // For some reason, if we haven't got one use the first root folder in Sonarr
                // TODO make this overrideable via the UI
                rootFolderPath = await GetSonarrRootPath(model.ParentRequest.RootFolder ?? int.Parse(s.RootPath), s);
                qualityToUse = model.ParentRequest.QualityOverride.Value;
            }
            
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
                        qualityProfileId = qualityToUse,
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
                    for (var i = 1; i < model.ParentRequest.TotalSeasons + 1; i++)
                    {
                        var index = i;
                        var season = new Season
                        {
                            seasonNumber = i,
                            monitored = model.SeasonRequests.Any(x => x.SeasonNumber == index)
                        };
                        seasonsToAdd.Add(season);
                    }
                    newSeries.seasons = seasonsToAdd;
                    var result = await SonarrApi.AddSeries(newSeries, s.ApiKey, s.FullUri);
                    existingSeries = await SonarrApi.GetSeriesById(result.id, s.ApiKey, s.FullUri);
                    await SendToSonarr(model, existingSeries, s);
                }
                else
                {
                    await SendToSonarr(model, existingSeries, s);
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

        private async Task SendToSonarr(ChildRequests model, SonarrSeries result, SonarrSettings s)
        {
            var episodesToUpdate = new List<Episode>();
            // Ok, now let's sort out the episodes.

            if (model.SeriesType == SeriesType.Anime)
            {
                result.seriesType = "anime";
                await SonarrApi.UpdateSeries(result, s.ApiKey, s.FullUri);
            }

            var sonarrEpisodes = await SonarrApi.GetEpisodes(result.id, s.ApiKey, s.FullUri);
            var sonarrEpList = sonarrEpisodes.ToList() ?? new List<Episode>();
            while (!sonarrEpList.Any())
            {
                // It could be that the series metadata is not ready yet. So wait
                sonarrEpList = (await SonarrApi.GetEpisodes(result.id, s.ApiKey, s.FullUri)).ToList();
                await Task.Delay(500);
            }


            foreach (var req in model.SeasonRequests)
            {
                foreach (var ep in req.Episodes)
                {
                    var sonarrEp = sonarrEpList.FirstOrDefault(x =>
                        x.episodeNumber == ep.EpisodeNumber && x.seasonNumber == req.SeasonNumber);
                    if (sonarrEp != null)
                    {
                        sonarrEp.monitored = true;
                        episodesToUpdate.Add(sonarrEp);
                    }
                }
            }
            var seriesChanges = false;
            foreach (var season in model.SeasonRequests)
            {
                var sonarrSeason = sonarrEpList.Where(x => x.seasonNumber == season.SeasonNumber);
                var sonarrEpCount = sonarrSeason.Count();
                var ourRequestCount = season.Episodes.Count;

                if (sonarrEpCount == ourRequestCount)
                {
                    // We have the same amount of requests as all of the episodes in the season.
                    var existingSeason =
                        result.seasons.First(x => x.seasonNumber == season.SeasonNumber);
                    existingSeason.monitored = true;
                    seriesChanges = true;
                }
                else
                {
                    // Now update the episodes that need updating
                    foreach (var epToUpdate in episodesToUpdate.Where(x => x.seasonNumber == season.SeasonNumber))
                    {
                        await SonarrApi.UpdateEpisode(epToUpdate, s.ApiKey, s.FullUri);
                    }
                }
            }
            if (seriesChanges)
            {
                await SonarrApi.SeasonPass(s.ApiKey, s.FullUri, result);
            }


            if (!s.AddOnly)
            {
                await SearchForRequest(model, sonarrEpList, result, s, episodesToUpdate);
            }
        }

        private async Task<bool> SendToSickRage(ChildRequests model, SickRageSettings settings, string qualityId = null)
        {
            var tvdbid = model.ParentRequest.TvDbId;
            if (qualityId.HasValue())
            {
                var id = qualityId;
                if (settings.Qualities.All(x => x.Value != id))
                {
                    qualityId = settings.QualityProfile;
                }
            }
            else
            {
                qualityId = settings.QualityProfile;
            }
            // Check if the show exists
            var existingShow = await SickRageApi.GetShow(tvdbid, settings.ApiKey, settings.FullUri);

            if (existingShow.message.Equals("Show not found", StringComparison.CurrentCultureIgnoreCase))
            {
                var addResult = await SickRageApi.AddSeries(model.ParentRequest.TvDbId, qualityId, SickRageStatus.Ignored,
                    settings.ApiKey, settings.FullUri);

                Logger.LogDebug("Added the show (tvdbid) {0}. The result is '{2}' : '{3}'", tvdbid, addResult.result, addResult.message);
                if (addResult.result.Equals("failure") || addResult.result.Equals("fatal"))
                {
                    // Do something
                    return false;
                }
            }

            foreach (var seasonRequests in model.SeasonRequests)
            {
                var srEpisodes = await SickRageApi.GetEpisodesForSeason(tvdbid, seasonRequests.SeasonNumber, settings.ApiKey, settings.FullUri);
                while (srEpisodes.message.Equals("Show not found", StringComparison.CurrentCultureIgnoreCase) && srEpisodes.data.Count <= 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    srEpisodes = await SickRageApi.GetEpisodesForSeason(tvdbid, seasonRequests.SeasonNumber, settings.ApiKey, settings.FullUri);
                }

                var totalSrEpisodes = srEpisodes.data.Count;

                if (totalSrEpisodes == seasonRequests.Episodes.Count)
                {
                    // This is a request for the whole season
                    var wholeSeasonResult = await SickRageApi.SetEpisodeStatus(settings.ApiKey, settings.FullUri, tvdbid, SickRageStatus.Wanted,
                        seasonRequests.SeasonNumber);

                    Logger.LogDebug("Set the status to Wanted for season {0}. The result is '{1}' : '{2}'", seasonRequests.SeasonNumber, wholeSeasonResult.result, wholeSeasonResult.message);
                    continue;
                }

                foreach (var srEp in srEpisodes.data)
                {
                    var epNumber = srEp.Key;
                    var epData = srEp.Value;

                    var epRequest = seasonRequests.Episodes.FirstOrDefault(x => x.EpisodeNumber == epNumber);
                    if (epRequest != null)
                    {
                        // We want to monior this episode since we have a request for it
                        // Let's check to see if it's wanted first, save an api call
                        if (epData.status.Equals(SickRageStatus.Wanted, StringComparison.CurrentCultureIgnoreCase))
                        {
                            continue;
                        }
                        var epResult = await SickRageApi.SetEpisodeStatus(settings.ApiKey, settings.FullUri, tvdbid,
                            SickRageStatus.Wanted, seasonRequests.SeasonNumber, epNumber);

                        Logger.LogDebug("Set the status to Wanted for Episode {0} in season {1}. The result is '{2}' : '{3}'", seasonRequests.SeasonNumber, epNumber, epResult.result, epResult.message);
                    }
                }
            }
            return true;
        }

        private async Task SearchForRequest(ChildRequests model, IEnumerable<Episode> sonarrEpList, SonarrSeries existingSeries, SonarrSettings s,
            IReadOnlyCollection<Episode> episodesToUpdate)
        {
            foreach (var season in model.SeasonRequests)
            {
                var sonarrSeason = sonarrEpList.Where(x => x.seasonNumber == season.SeasonNumber);
                var sonarrEpCount = sonarrSeason.Count();
                var ourRequestCount = season.Episodes.Count;

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