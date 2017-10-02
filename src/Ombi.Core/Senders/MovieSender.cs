using System.Linq;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models.External;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Radarr;
using Ombi.Helpers;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core
{
    public class MovieSender : IMovieSender
    {
        public MovieSender(ISettingsService<RadarrSettings> radarrSettings, IRadarrApi api, ILogger<MovieSender> log)
        {
            RadarrSettings = radarrSettings;
            RadarrApi = api;
            Log = log;
        }

        private ISettingsService<RadarrSettings> RadarrSettings { get; }
        private IRadarrApi RadarrApi { get; }
        private ILogger<MovieSender> Log { get; }

        public async Task<MovieSenderResult> Send(MovieRequests model)
        {
            //var cpSettings = await CouchPotatoSettings.GetSettingsAsync();
            //var watcherSettings = await WatcherSettings.GetSettingsAsync();
            var radarrSettings = await RadarrSettings.GetSettingsAsync();

            //if (cpSettings.Enabled)
            //{
            //    return SendToCp(model, cpSettings, string.IsNullOrEmpty(qualityId) ? cpSettings.ProfileId : qualityId);
            //}

            //if (watcherSettings.Enabled)
            //{
            //    return SendToWatcher(model, watcherSettings);
            //}

            if (radarrSettings.Enabled)
            {
                return await SendToRadarr(model, radarrSettings);
            }

            return new MovieSenderResult
            {
                Success = true,
                MovieSent = false,
            };
        }

        private async Task<MovieSenderResult> SendToRadarr(MovieRequests model, RadarrSettings settings)
        {
            var qualityToUse = int.Parse(settings.DefaultQualityProfile);
            if (model.QualityOverride <= 0)
            {
                qualityToUse = model.QualityOverride;
            }

            var rootFolderPath = model.RootPathOverride <= 0 ? settings.DefaultRootPath : await RadarrRootPath(model.RootPathOverride, settings);
            var result = await RadarrApi.AddMovie(model.TheMovieDbId, model.Title, model.ReleaseDate.Year, qualityToUse, rootFolderPath, settings.ApiKey, settings.FullUri, !settings.AddOnly, settings.MinimumAvailability);

            if (!string.IsNullOrEmpty(result.Error?.message))
            {
                Log.LogError(LoggingEvents.RadarrCacher,result.Error.message);
                return new MovieSenderResult { Success = false, Message = result.Error.message, MovieSent = false };
            }
            if (!string.IsNullOrEmpty(result.title))
            {
                return new MovieSenderResult { Success = true, MovieSent = false };
            }
            return new MovieSenderResult { Success = true, MovieSent = false };
        }

        private async Task<string> RadarrRootPath(int overrideId, RadarrSettings settings)
        {
            var paths = await RadarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
            var selectedPath = paths.FirstOrDefault(x => x.id == overrideId);
            return selectedPath.path;
        }
    }
}