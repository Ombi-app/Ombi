using Microsoft.Extensions.Logging;
using Ombi.Api.Sonarr;
using Ombi.Api.Sonarr.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Store.Entities.Requests;
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
        public async Task<NewSeries> SendToSonarr(ChildRequests model, int totalSeasons, string qualityId = null)
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


            // Does the series actually exist?
            var allSeries = await SonarrApi.GetSeries(s.ApiKey, s.FullUri);
            var existingSeries = allSeries.FirstOrDefault(x => x.tvdbId == model.ParentRequest.TvDbId);

            if(existingSeries == null)
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
                        ignoreEpisodesWithoutFiles = false, // We want all missing
                        searchForMissingEpisodes = true // we want to search for missing TODO pass this in
                    }
                };

                // Montitor the correct seasons,
                // If we have that season in the model then it's monitored!
                var seasonsToAdd = new List<Season>();
                for (int i = 1; i < totalSeasons; i++)
                {
                    var season = new Season
                    {
                        seasonNumber = i,
                        monitored = model.SeasonRequests.Any(x => x.SeasonNumber == i)
                    };
                    seasonsToAdd.Add(season);
                }

                var result = SonarrApi.AddSeries(newSeries, s.ApiKey, s.FullUri);
                return result;
            }
            else
            {
                // Let's update the existing
            }



            
            return null;
        }

        private async Task<string> GetSonarrRootPath(int pathId, SonarrSettings sonarrSettings)
        {
            var rootFoldersResult = await SonarrApi.GetRootFolders(sonarrSettings.ApiKey, sonarrSettings.FullUri);

            if(pathId == 0)
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