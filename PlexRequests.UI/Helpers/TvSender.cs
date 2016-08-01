#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: TvSender.cs
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
using System.Diagnostics;

using NLog;
using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.SickRage;
using PlexRequests.Api.Models.Sonarr;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store;
using System.Linq;
using System.Threading.Tasks;

using PlexRequests.Helpers.Exceptions;

namespace PlexRequests.UI.Helpers
{
    public class TvSender
    {
        public TvSender(ISonarrApi sonarrApi, ISickRageApi srApi)
        {
            SonarrApi = sonarrApi;
            SickrageApi = srApi;
        }
        private ISonarrApi SonarrApi { get; }
        private ISickRageApi SickrageApi { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public async Task<SonarrAddSeries> SendToSonarr(SonarrSettings sonarrSettings, RequestedModel model)
        {
            return await SendToSonarr(sonarrSettings, model, string.Empty);
        }

        public async Task<SonarrAddSeries> SendToSonarr(SonarrSettings sonarrSettings, RequestedModel model, string qualityId)
        {
            var qualityProfile = 0;
            var episodeRequest = model.Episodes.Length > 0;
            if (!string.IsNullOrEmpty(qualityId)) // try to parse the passed in quality, otherwise use the settings default quality
            {
                int.TryParse(qualityId, out qualityProfile);
            }

            if (qualityProfile <= 0)
            {
                int.TryParse(sonarrSettings.QualityProfile, out qualityProfile);
            }


            var seriesTask = GetSonarrSeries(sonarrSettings, model.ProviderId);

            if (episodeRequest)
            {            
                // Does series exist?
                var series = await seriesTask;
                if (series != null)
                {
                    // Series Exists
                    // Request the episodes in the existing series
                    await RequestEpisodesWithExistingSeries(model, series, sonarrSettings);
                }
                else
                {
                    // Series doesn't exist, need to add it as unmonitored.
                    var addResult = await Task.Run(() => SonarrApi.AddSeries(model.ProviderId, model.Title, qualityProfile,
                            sonarrSettings.SeasonFolders, sonarrSettings.RootPath, 0, new int[0], sonarrSettings.ApiKey,
                            sonarrSettings.FullUri, false));

                    if (string.IsNullOrEmpty(addResult?.title))
                    {
                        var sw = new Stopwatch();
                        sw.Start();
                        while (series == null)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(5));
                            series = await GetSonarrSeries(sonarrSettings, model.ProviderId);

                            // Check how long we have been doing this for
                            if (sw.Elapsed > TimeSpan.FromSeconds(30))
                            {
                                // 30 seconds is a long time, it's not going to work.
                                throw new ApiRequestException("Sonarr still didn't have the series added after 30 seconds.");
                            }
                        }
                        sw.Stop();

                        // Update the series, Since we cannot add as unmonitoed due to the following bug: https://github.com/Sonarr/Sonarr/issues/1404
                        SonarrApi.UpdateSeries(series, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                    }

                    // We now have the series in Sonarr
                    await RequestEpisodesWithExistingSeries(model, series, sonarrSettings);

                    return addResult;
                }
            }


            var result = SonarrApi.AddSeries(model.ProviderId, model.Title, qualityProfile,
                sonarrSettings.SeasonFolders, sonarrSettings.RootPath, model.SeasonCount, model.SeasonList, sonarrSettings.ApiKey,
                sonarrSettings.FullUri);

            return result;
        }

        public SickRageTvAdd SendToSickRage(SickRageSettings sickRageSettings, RequestedModel model)
        {
            return SendToSickRage(sickRageSettings, model, sickRageSettings.QualityProfile);
        }

        public SickRageTvAdd SendToSickRage(SickRageSettings sickRageSettings, RequestedModel model, string qualityId)
        {
            Log.Info("Sending to SickRage {0}", model.Title);
            if (!sickRageSettings.Qualities.Any(x => x.Key == qualityId))
            {
                qualityId = sickRageSettings.QualityProfile;
            }

            var apiResult = SickrageApi.AddSeries(model.ProviderId, model.SeasonCount, model.SeasonList, qualityId,
                               sickRageSettings.ApiKey, sickRageSettings.FullUri);

            var result = apiResult.Result;


            return result;
        }

        private async Task RequestEpisodesWithExistingSeries(RequestedModel model, Series selectedSeries, SonarrSettings sonarrSettings)
        {
            // Show Exists
            // Look up all episodes
            var episodes = SonarrApi.GetEpisodes(selectedSeries.id.ToString(), sonarrSettings.ApiKey, sonarrSettings.FullUri).ToList();
            var internalEpisodeIds = new List<int>();
            var tasks = new List<Task>();
            foreach (var r in model.Episodes)
            {
                // Match the episode and season number.
                // Also we need to make sure that the episode is not monitored already, otherwise there is no point.
                var episode =
                    episodes.FirstOrDefault(
                        x => x.episodeNumber == r.EpisodeNumber && x.seasonNumber == r.SeasonNumber && !x.monitored);
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
        private async Task<Series> GetSonarrSeries(SonarrSettings sonarrSettings, int showId)
        {
            var task = await Task.Run(() => SonarrApi.GetSeries(sonarrSettings.ApiKey, sonarrSettings.FullUri)).ConfigureAwait(false);
            var selectedSeries = task.FirstOrDefault(series => series.tvdbId == showId);

            return selectedSeries;
        }
    }
}