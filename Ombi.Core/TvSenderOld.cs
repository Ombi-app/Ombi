#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: TvSenderOld.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.SickRage;
using Ombi.Api.Models.Sonarr;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Store;

namespace Ombi.Core
{
    public class TvSenderOld
    {
        public TvSenderOld(ISonarrApi sonarrApi, ISickRageApi srApi, ICacheProvider cache)
        {
            SonarrApi = sonarrApi;
            SickrageApi = srApi;
            Cache = cache;
        }
        private ISonarrApi SonarrApi { get; }
        private ISickRageApi SickrageApi { get; }
        private ICacheProvider Cache { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public async Task<SonarrAddSeries> SendToSonarr(SonarrSettings sonarrSettings, RequestedModel model)
        {
            return await SendToSonarr(sonarrSettings, model, string.Empty);
        }

        public async Task<SonarrAddSeries> SendToSonarr(SonarrSettings sonarrSettings, RequestedModel model, string qualityId)
        {
            var qualityProfile = 0;
            var episodeRequest = model.Episodes.Any();
            if (!string.IsNullOrEmpty(qualityId)) // try to parse the passed in quality, otherwise use the settings default quality
            {
                int.TryParse(qualityId, out qualityProfile);
            }

            if (qualityProfile <= 0)
            {
                int.TryParse(sonarrSettings.QualityProfile, out qualityProfile);
            }
            var rootFolderPath = model.RootFolderSelected <= 0 ? sonarrSettings.RootPath : await GetRootPath(model.RootFolderSelected, sonarrSettings);


            var series = await GetSonarrSeries(sonarrSettings, model.ProviderId);

            if (episodeRequest)
            {
                // Does series exist?
                if (series != null)
                {
                    // Series Exists
                    // Request the episodes in the existing series
                    await RequestEpisodesWithExistingSeries(model, series, sonarrSettings);
                    return new SonarrAddSeries { title = series.title };
                }


                // Series doesn't exist, need to add it as unmonitored.
                var addResult = await Task.Run(() => SonarrApi.AddSeries(model.ProviderId, model.Title, qualityProfile,
                    sonarrSettings.SeasonFolders, rootFolderPath, 0, new int[0], sonarrSettings.ApiKey,
                    sonarrSettings.FullUri, false));


                // Get the series that was just added
                series = await GetSonarrSeries(sonarrSettings, model.ProviderId);
                series.monitored = true; // We want to make sure we are monitoring the series

                // Un-monitor all seasons
                foreach (var season in series.seasons)
                {
                    season.monitored = false;
                }

                // Update the series, Since we cannot add as un-monitored due to the following bug: https://github.com/Sonarr/Sonarr/issues/1404
                SonarrApi.UpdateSeries(series, sonarrSettings.ApiKey, sonarrSettings.FullUri);


                // We now have the series in Sonarr, update it to request the episodes.
                await RequestAllEpisodesInASeasonWithExistingSeries(model, series, sonarrSettings);

                return addResult;
            }

            if (series != null)
            {
                var requestAll = model.SeasonsRequested.Equals("All", StringComparison.CurrentCultureIgnoreCase);
                var first = model.SeasonsRequested.Equals("First", StringComparison.CurrentCultureIgnoreCase);
                var latest = model.SeasonsRequested.Equals("Latest", StringComparison.CurrentCultureIgnoreCase);

                if (model.SeasonList.Any())
                {
                    // Monitor the seasons that we have chosen
                    foreach (var season in series.seasons)
                    {
                        if (model.SeasonList.Contains(season.seasonNumber))
                        {
                            season.monitored = true;
                        }
                    }
                }

                if (requestAll)
                {
                    // Monitor all seasons
                    foreach (var season in series.seasons)
                    {
                        season.monitored = true;
                    }
                }

                if (first)
                {
                    var firstSeries = series?.seasons?.OrderBy(x => x.seasonNumber)?.FirstOrDefault() ?? new Season();
                    firstSeries.monitored = true;
                }

                if (latest)
                {
                    var lastSeries = series?.seasons?.OrderByDescending(x => x.seasonNumber)?.FirstOrDefault() ?? new Season();
                    lastSeries.monitored = true;
                }


                // Update the series in sonarr with the new monitored status
                SonarrApi.UpdateSeries(series, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                await RequestAllEpisodesInASeasonWithExistingSeries(model, series, sonarrSettings);
                return new SonarrAddSeries { title = series.title }; // We have updated it
            }


            var result = SonarrApi.AddSeries(model.ProviderId, model.Title, qualityProfile,
                sonarrSettings.SeasonFolders, rootFolderPath, model.SeasonCount, model.SeasonList, sonarrSettings.ApiKey,
                sonarrSettings.FullUri, true, true);

            return result;
        }

        public SickRageTvAdd SendToSickRage(SickRageSettings sickRageSettings, RequestedModel model)
        {
            return SendToSickRage(sickRageSettings, model, sickRageSettings.QualityProfile);
        }

        public SickRageTvAdd SendToSickRage(SickRageSettings sickRageSettings, RequestedModel model, string qualityId)
        {
            Log.Info("Sending to SickRage {0}", model.Title);
            if (sickRageSettings.Qualities.All(x => x.Key != qualityId))
            {
                qualityId = sickRageSettings.QualityProfile;
            }

            var apiResult = SickrageApi.AddSeries(model.ProviderId, model.SeasonCount, model.SeasonList, qualityId,
                               sickRageSettings.ApiKey, sickRageSettings.FullUri);

            var result = apiResult.Result;


            return result;
        }

        internal async Task RequestEpisodesWithExistingSeries(RequestedModel model, Series selectedSeries, SonarrSettings sonarrSettings)
        {
            // Show Exists
            // Look up all episodes
            var ep = SonarrApi.GetEpisodes(selectedSeries.id.ToString(), sonarrSettings.ApiKey, sonarrSettings.FullUri);
            var episodes = ep?.ToList() ?? new List<SonarrEpisodes>();

            var internalEpisodeIds = new List<int>();
            var tasks = new List<Task>();
            foreach (var r in model.Episodes)
            {
                // Match the episode and season number.
                // If the episode is monitored we might not be searching for it.
                var episode =
                    episodes.FirstOrDefault(
                        x => x.episodeNumber == r.EpisodeNumber && x.seasonNumber == r.SeasonNumber);
                if (episode == null)
                {
                    continue;
                }
                var episodeInfo = SonarrApi.GetEpisode(episode.id.ToString(), sonarrSettings.ApiKey, sonarrSettings.FullUri);
                episodeInfo.monitored = true; // Set the episode to monitored
                tasks.Add(Task.Run(() => SonarrApi.UpdateEpisode(episodeInfo, sonarrSettings.ApiKey,
                    sonarrSettings.FullUri)));
                internalEpisodeIds.Add(episode.id);
            }

            await Task.WhenAll(tasks.ToArray());

            SonarrApi.SearchForEpisodes(internalEpisodeIds.ToArray(), sonarrSettings.ApiKey, sonarrSettings.FullUri);
        }

        internal async Task RequestAllEpisodesWithExistingSeries(RequestedModel model, Series selectedSeries, SonarrSettings sonarrSettings)
        {
            // Show Exists
            // Look up all episodes
            var ep = SonarrApi.GetEpisodes(selectedSeries.id.ToString(), sonarrSettings.ApiKey, sonarrSettings.FullUri);
            var episodes = ep?.ToList() ?? new List<SonarrEpisodes>();

            var internalEpisodeIds = new List<int>();
            var tasks = new List<Task>();
            foreach (var r in episodes)
            {
                if (r.monitored || r.hasFile) // If it's already montiored or has the file, there is no point in updating it
                {
                    continue;
                }

                // Lookup the individual episode details
                var episodeInfo = SonarrApi.GetEpisode(r.id.ToString(), sonarrSettings.ApiKey, sonarrSettings.FullUri);
                episodeInfo.monitored = true; // Set the episode to monitored

                tasks.Add(Task.Run(() => SonarrApi.UpdateEpisode(episodeInfo, sonarrSettings.ApiKey,
                    sonarrSettings.FullUri)));
                internalEpisodeIds.Add(r.id);
            }

            await Task.WhenAll(tasks.ToArray());

            SonarrApi.SearchForEpisodes(internalEpisodeIds.ToArray(), sonarrSettings.ApiKey, sonarrSettings.FullUri);
        }


        internal async Task RequestAllEpisodesInASeasonWithExistingSeries(RequestedModel model, Series selectedSeries, SonarrSettings sonarrSettings)
        {
            // Show Exists
            // Look up all episodes
            var ep = SonarrApi.GetEpisodes(selectedSeries.id.ToString(), sonarrSettings.ApiKey, sonarrSettings.FullUri);
            var episodes = ep?.ToList() ?? new List<SonarrEpisodes>();

            var internalEpisodeIds = new List<int>();
            var tasks = new List<Task>();

            var requestedEpisodes = model.Episodes;

            foreach (var r in episodes)
            {
                if (r.hasFile) // If it already has the file, there is no point in updating it
                {
                    continue;
                }
                var epComparison = new EpisodesModel
                {
                    EpisodeNumber = r.episodeNumber,
                    SeasonNumber = r.seasonNumber
                };
                // Make sure we are looking for the right episode and season
                if (!requestedEpisodes.Contains(epComparison))
                {
                    continue;
                }

                // Lookup the individual episode details
                var episodeInfo = SonarrApi.GetEpisode(r.id.ToString(), sonarrSettings.ApiKey, sonarrSettings.FullUri);
                // If the season is not in thr

                episodeInfo.monitored = true; // Set the episode to monitored

                tasks.Add(Task.Run(() => SonarrApi.UpdateEpisode(episodeInfo, sonarrSettings.ApiKey,
                    sonarrSettings.FullUri)));
                internalEpisodeIds.Add(r.id);
            }

            await Task.WhenAll(tasks.ToArray());

            SonarrApi.SearchForEpisodes(internalEpisodeIds.ToArray(), sonarrSettings.ApiKey, sonarrSettings.FullUri);
        }
        private async Task<Series> GetSonarrSeries(SonarrSettings sonarrSettings, int showId)
        {
            var task = await Task.Run(() => SonarrApi.GetSeries(sonarrSettings.ApiKey, sonarrSettings.FullUri)).ConfigureAwait(false);
            var selectedSeries = task.FirstOrDefault(series => series.tvdbId == showId);

            return selectedSeries;
        }


        private async Task<string> GetRootPath(int pathId, SonarrSettings sonarrSettings)
        {
            var rootFoldersResult = await Cache.GetOrSetAsync(CacheKeys.SonarrRootFolders, async () =>
            {
                return await Task.Run(() => SonarrApi.GetRootFolders(sonarrSettings.ApiKey, sonarrSettings.FullUri));
            });

            foreach (var r in rootFoldersResult.Where(r => r.id == pathId))
            {
                return r.path;
            }
            return string.Empty;
        }
    }
}