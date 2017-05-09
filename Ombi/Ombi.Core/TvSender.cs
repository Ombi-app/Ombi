using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Sonarr;
using Ombi.Core.Models.Requests;
using Ombi.Core.Settings.Models.External;

namespace Ombi.Core
{
    public class TvSender
    {
        public TvSender(ISonarrApi sonarrApi, ILogger<TvSender> log)
        {
            SonarrApi = sonarrApi;
            Logger = log;
        }

        private ISonarrApi SonarrApi { get; }
        private ILogger<TvSender> Logger { get; }

        //public async Task<SonarrAddSeries> SendToSonarr(SonarrSettings sonarrSettings, TvRequestModel model,
        //    string qualityId)
        //{
        //    var qualityProfile = 0;
        //    if (!string.IsNullOrEmpty(qualityId)) // try to parse the passed in quality, otherwise use the settings default quality
        //    {
        //        int.TryParse(qualityId, out qualityProfile);
        //    }

        //    if (qualityProfile <= 0)
        //    {
        //        int.TryParse(sonarrSettings.QualityProfile, out qualityProfile);
        //    }
        //    var rootFolderPath = model.RootFolderSelected <= 0 ? sonarrSettings.FullRootPath : await GetSonarrRootPath(model.RootFolderSelected, sonarrSettings);

        //    //var episodeRequest = model.Episodes.Any();
        //    //var requestAll = model.SeasonsRequested?.Equals("All", StringComparison.CurrentCultureIgnoreCase);
        //    //var first = model.SeasonsRequested?.Equals("First", StringComparison.CurrentCultureIgnoreCase);
        //    //var latest = model.SeasonsRequested?.Equals("Latest", StringComparison.CurrentCultureIgnoreCase);
        //    //var specificSeasonRequest = model.SeasonList?.Any();

        //    //if (episodeRequest)
        //    //{
        //    //    return await ProcessSonarrEpisodeRequest(sonarrSettings, model, qualityProfile, rootFolderPath);
        //    //}

        //    //if (requestAll ?? false)
        //    //{
        //    //    return await ProcessSonarrRequestSeason(sonarrSettings, model, qualityProfile, rootFolderPath);
        //    //}

        //    //if (first ?? false)
        //    //{
        //    //    return await ProcessSonarrRequestSeason(sonarrSettings, model, qualityProfile, rootFolderPath);
        //    //}

        //    //if (latest ?? false)
        //    //{
        //    //    return await ProcessSonarrRequestSeason(sonarrSettings, model, qualityProfile, rootFolderPath);
        //    //}

        //    //if (specificSeasonRequest ?? false)
        //    //{
        //    //    return await ProcessSonarrRequestSeason(sonarrSettings, model, qualityProfile, rootFolderPath);
        //    //}

        //    return null;
        //}

        private async Task<string> GetSonarrRootPath(int pathId, SonarrSettings sonarrSettings)
        {
            var rootFoldersResult = await SonarrApi.GetRootFolders(sonarrSettings.ApiKey, sonarrSettings.FullUri);
            
            foreach (var r in rootFoldersResult.Where(r => r.id == pathId))
            {
                return r.path;
            }
            return string.Empty;
        }


    }
}