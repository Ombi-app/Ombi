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
using NLog;
using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.SickRage;
using PlexRequests.Api.Models.Sonarr;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store;
using System.Linq;
using System.Threading.Tasks;

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

        /// <summary>
        /// Broken Way 
        /// </summary>
        /// <param name="sonarrSettings"></param>
        /// <param name="model"></param>
        /// <param name="qualityId"></param>
        /// <returns></returns>
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

            var series = await GetSonarrSeries(sonarrSettings, model.ProviderId);

            var requestAll = model.SeasonsRequested?.Equals("All", StringComparison.CurrentCultureIgnoreCase);
            var first = model.SeasonsRequested?.Equals("First", StringComparison.CurrentCultureIgnoreCase);
            var latest = model.SeasonsRequested?.Equals("Latest", StringComparison.CurrentCultureIgnoreCase);
            var specificSeasonRequest = model.SeasonList?.Any();

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
                    sonarrSettings.SeasonFolders, sonarrSettings.RootPath, 0, new int[0], sonarrSettings.ApiKey,
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

            // Series exists, don't need to add it
            if (series == null)
            {
                // Set the series as monitored with a season count as 0 so it doesn't search for anything
                SonarrApi.AddSeriesNew(model.ProviderId, model.Title, qualityProfile,
                    sonarrSettings.SeasonFolders, sonarrSettings.RootPath, new int[] {1,2,3,4,5,6,7,8,9,10,11,12,13}, sonarrSettings.ApiKey,
                    sonarrSettings.FullUri);

                await Task.Delay(TimeSpan.FromSeconds(1));

                series = await GetSonarrSeries(sonarrSettings, model.ProviderId);

                
                foreach (var s in series.seasons)
                {
                    s.monitored = false;
                }

                SonarrApi.UpdateSeries(series, sonarrSettings.ApiKey, sonarrSettings.FullUri);
            }

            if (requestAll ?? false)
            {
                // Monitor all seasons
                foreach (var season in series.seasons)
                {
                    season.monitored = true;
                }


                SonarrApi.UpdateSeries(series, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                SonarrApi.SearchForSeries(series.id, sonarrSettings.ApiKey, sonarrSettings.FullUri); // Search For all episodes!"


                //// This is a work around for this issue: https://github.com/Sonarr/Sonarr/issues/1507
                //// The above is the previous code.
                //SonarrApi.AddSeries(model.ProviderId, model.Title, qualityProfile,
                //    sonarrSettings.SeasonFolders, sonarrSettings.RootPath, 0, model.SeasonList, sonarrSettings.ApiKey,
                //    sonarrSettings.FullUri, true, true);
                return new SonarrAddSeries { title = series.title }; // We have updated it
            }

            

            if (first ?? false)
            {
                var firstSeries = (series?.seasons?.OrderBy(x => x.seasonNumber)).FirstOrDefault(x => x.seasonNumber > 0) ?? new Season();
                firstSeries.monitored = true;
                var episodes = SonarrApi.GetEpisodes(series.id.ToString(), sonarrSettings.ApiKey, sonarrSettings.FullUri); // Need to get the episodes so we mark them as monitored

                var episodesToUpdate = new List<SonarrEpisodes>();
                foreach (var e in episodes)
                {
                    if (e.hasFile || e.seasonNumber != firstSeries.seasonNumber)
                    {
                        continue;
                    }
                    e.monitored = true; // Mark only the episodes we want as monitored
                    episodesToUpdate.Add(e);
                }
                foreach (var sonarrEpisode in episodesToUpdate)
                {
                    SonarrApi.UpdateEpisode(sonarrEpisode, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                }

                SonarrApi.UpdateSeries(series, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                SonarrApi.SearchForSeason(series.id, firstSeries.seasonNumber, sonarrSettings.ApiKey,
                    sonarrSettings.FullUri);
                return new SonarrAddSeries { title = series.title }; // We have updated it
            }

            if (latest ?? false)
            {
                var lastSeries = series?.seasons?.OrderByDescending(x => x.seasonNumber)?.FirstOrDefault() ?? new Season();
                lastSeries.monitored = true;

                var episodes = SonarrApi.GetEpisodes(series.id.ToString(), sonarrSettings.ApiKey, sonarrSettings.FullUri); // Need to get the episodes so we mark them as monitored

                var episodesToUpdate = new List<SonarrEpisodes>();
                foreach (var e in episodes)
                {
                    if (e.hasFile || e.seasonNumber != lastSeries.seasonNumber)
                    {
                        continue;
                    }
                    e.monitored = true; // Mark only the episodes we want as monitored
                    episodesToUpdate.Add(e);
                }
                foreach (var sonarrEpisode in episodesToUpdate)
                {
                    SonarrApi.UpdateEpisode(sonarrEpisode, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                }
                SonarrApi.UpdateSeries(series, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                SonarrApi.SearchForSeason(series.id, lastSeries.seasonNumber, sonarrSettings.ApiKey,
                    sonarrSettings.FullUri);
                return new SonarrAddSeries { title = series.title }; // We have updated it
            }

            if (specificSeasonRequest ?? false)
            {
                // Monitor the seasons that we have chosen
                foreach (var season in series.seasons)
                {
                    if (model.SeasonList.Contains(season.seasonNumber))
                    {
                        season.monitored = true;
                        SonarrApi.UpdateSeries(series, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                        SonarrApi.SearchForSeason(series.id, season.seasonNumber, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                    }
                }
                return new SonarrAddSeries { title = series.title }; // We have updated it
            }

            return null;
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
                if (r.monitored || r.hasFile) // If it's already monitored or has the file, there is no point in updating it
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
    }
}