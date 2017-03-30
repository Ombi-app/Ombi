#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: TvSenderV2.cs
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

namespace Ombi.Core.Tv
{
    public class TvSenderV2
    {
        public TvSenderV2(ISonarrApi sonarrApi, ISickRageApi srApi, ICacheProvider cache)
        {
            SonarrApi = sonarrApi;
            SickrageApi = srApi;
            Cache = cache;
        }
        private ISonarrApi SonarrApi { get; }
        private ISickRageApi SickrageApi { get; }
        private ICacheProvider Cache { get; }
        private static Logger _log = LogManager.GetCurrentClassLogger();


        public async Task<SonarrAddSeries> SendToSonarr(SonarrSettings sonarrSettings, RequestedModel model)
        {
            return await SendToSonarr(sonarrSettings, model, string.Empty);
        }


        public async Task<SonarrAddSeries> SendToSonarr(SonarrSettings sonarrSettings, RequestedModel model,
            string qualityId)
        {
            var qualityProfile = 0;
            if (!string.IsNullOrEmpty(qualityId)) // try to parse the passed in quality, otherwise use the settings default quality
            {
                int.TryParse(qualityId, out qualityProfile);
            }

            if (qualityProfile <= 0)
            {
                int.TryParse(sonarrSettings.QualityProfile, out qualityProfile);
            }
            var rootFolderPath = model.RootFolderSelected <= 0 ? sonarrSettings.FullRootPath : await GetSonarrRootPath(model.RootFolderSelected, sonarrSettings);

            var episodeRequest = model.Episodes.Any();
            var requestAll = model.SeasonsRequested?.Equals("All", StringComparison.CurrentCultureIgnoreCase);
            var first = model.SeasonsRequested?.Equals("First", StringComparison.CurrentCultureIgnoreCase);
            var latest = model.SeasonsRequested?.Equals("Latest", StringComparison.CurrentCultureIgnoreCase);
            var specificSeasonRequest = model.SeasonList?.Any();

            if (episodeRequest)
            {
                return await ProcessSonarrEpisodeRequest(sonarrSettings, model, qualityProfile, rootFolderPath);
            }

            if (requestAll ?? false)
            {
                return await ProcessSonarrRequestAll(sonarrSettings, model, qualityProfile, rootFolderPath);
            }

            if (first ?? false)
            {
                return await ProcessSonarrRequestFirstSeason(sonarrSettings, model, qualityProfile, rootFolderPath);
            }

            if (latest ?? false)
            {
                return await ProcessSonarrRequestLatestSeason(sonarrSettings, model, qualityProfile, rootFolderPath);
            }

            if (specificSeasonRequest ?? false)
            {
                return await ProcessSonarrRequestSpecificSeason(sonarrSettings, model, qualityProfile, rootFolderPath);
            }

            return null;
        }

        private async Task<SonarrAddSeries> ProcessSonarrRequestSpecificSeason(SonarrSettings sonarrSettings, RequestedModel model, int qualityId, string rootFolderPath)
        {
            throw new NotImplementedException();
        }

        private async Task<SonarrAddSeries> ProcessSonarrRequestLatestSeason(SonarrSettings sonarrSettings, RequestedModel model, int qualityId, string rootFolderPath)
        {
            // Does the series exist?

            var series = await GetSonarrSeries(sonarrSettings, model.ProviderId);
            if (series == null)
            {
                //WORKS
                // Add the series
                var seriesToAdd = new SonarrAddSeries
                {
                    seasonFolder = sonarrSettings.SeasonFolders,
                    title = model.Title,
                    qualityProfileId = qualityId,
                    tvdbId = model.ProviderId,
                    titleSlug = model.Title,
                    seasons = new List<Season>(),
                    rootFolderPath = rootFolderPath,
                    monitored = true, // Montior the series
                    images = new List<SonarrImage>(),
                    addOptions = new AddOptions
                    {
                        ignoreEpisodesWithFiles = true, // We don't really care about these
                        ignoreEpisodesWithoutFiles = false, // We want to get the whole season
                        searchForMissingEpisodes = true // we want to search for missing
                    }
                };

                for (var i = 1; i <= model.SeasonCount; i++)
                {
                    var season = new Season
                    {
                        seasonNumber = i,
                        // ReSharper disable once SimplifyConditionalTernaryExpression
                        monitored = true ? model.SeasonList.Length == 0 || model.SeasonList.Any(x => x == i) : false
                    };
                    seriesToAdd.seasons.Add(season);
                }

                return SonarrApi.AddSeries(seriesToAdd, sonarrSettings.ApiKey, sonarrSettings.FullUri);
            }
            else
            {
                // Mark the latest as monitored and search
                // Also make sure the series is now monitored otherwise we won't search for it
                series.monitored = true;
                foreach (var seasons in series.seasons)
                {
                    if (model.SeasonList.Any(x => x == seasons.seasonNumber))
                    {
                        seasons.monitored = true;
                    }
                }

                // Send the update command
                series = SonarrApi.UpdateSeries(series, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                SonarrApi.SearchForSeries(series.id, sonarrSettings.ApiKey, sonarrSettings.FullUri);
                return new SonarrAddSeries{title =  series.title};
            }
        }

        private async Task<SonarrAddSeries> ProcessSonarrRequestFirstSeason(SonarrSettings sonarrSettings, RequestedModel model, int qualityId, string rootFolderPath)
        {
            throw new NotImplementedException();
        }

        private async Task<SonarrAddSeries> ProcessSonarrRequestAll(SonarrSettings sonarrSettings, RequestedModel model, int qualityId, string rootFolderPath)
        {
            throw new NotImplementedException();
        }

        private async Task<SonarrAddSeries> ProcessSonarrEpisodeRequest(SonarrSettings sonarrSettings, RequestedModel model, int qualityId, string rootFolderPath)
        {
            // Does the series exist?

            var series = await GetSonarrSeries(sonarrSettings, model.ProviderId);
            if (series == null)
            {
                // Add the series
            }

            return null;
        }

        public SickRageTvAdd SendToSickRage(SickRageSettings sickRageSettings, RequestedModel model)
        {
            return SendToSickRage(sickRageSettings, model, sickRageSettings.QualityProfile);
        }

        public SickRageTvAdd SendToSickRage(SickRageSettings sickRageSettings, RequestedModel model, string qualityId)
        {
            _log.Info("Sending to SickRage {0}", model.Title);
            if (sickRageSettings.Qualities.All(x => x.Key != qualityId))
            {
                qualityId = sickRageSettings.QualityProfile;
            }

            var apiResult = SickrageApi.AddSeries(model.ProviderId, model.SeasonCount, model.SeasonList, qualityId,
                               sickRageSettings.ApiKey, sickRageSettings.FullUri);

            var result = apiResult.Result;


            return result;
        }

        private async Task<Series> GetSonarrSeries(SonarrSettings sonarrSettings, int showId)
        {
            var task = await Task.Run(() => SonarrApi.GetSeries(sonarrSettings.ApiKey, sonarrSettings.FullUri)).ConfigureAwait(false);
            var selectedSeries = task.FirstOrDefault(series => series.tvdbId == showId);

            return selectedSeries;
        }

        private async Task<string> GetSonarrRootPath(int pathId, SonarrSettings sonarrSettings)
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