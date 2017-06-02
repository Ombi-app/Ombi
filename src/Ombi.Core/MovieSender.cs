using Ombi.Core.Models.Requests.Movie;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models.External;
using System.Threading.Tasks;

namespace Ombi.Core
{
    public class MovieSender
    {
        public MovieSender(ISettingsService<RadarrSettings> radarrSettings)
        {
            RadarrSettings = radarrSettings;
        }

        private ISettingsService<RadarrSettings> RadarrSettings { get; }

        public async Task<MovieSenderResult> Send(MovieRequestModel model, string qualityId = "")
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
                //return SendToRadarr(model, radarrSettings, qualityId);
            }

            return new MovieSenderResult
            {
                Success = false,
                MovieSent = false,
                Message = "There are no movie providers enabled!"
            };
        }

        //    var qualityProfile = 0;
        //{
        //private MovieSenderResult SendToRadarr(MovieRequestModel model, RadarrSettings settings, string qualityId)
        //    if (!string.IsNullOrEmpty(qualityId)) // try to parse the passed in quality, otherwise use the settings default quality
        //    {
        //        int.TryParse(qualityId, out qualityProfile);
        //    }

        //    if (qualityProfile <= 0)
        //    {
        //        int.TryParse(settings.QualityProfile, out qualityProfile);
        //    }

        //    var rootFolderPath = model.RootFolderSelected <= 0 ? settings.FullRootPath : GetRootPath(model.RootFolderSelected, settings);
        //    var result = RadarrApi.AddMovie(model.ProviderId, model.Title, model.ReleaseDate.Year, qualityProfile, rootFolderPath, settings.ApiKey, settings.FullUri, true);

        //    if (!string.IsNullOrEmpty(result.Error?.message))
        //    {
        //        Log.Error(result.Error.message);
        //        return new MovieSenderResult { Result = false, Error = true, MovieSendingEnabled = true };
        //    }
        //    if (!string.IsNullOrEmpty(result.title))
        //    {
        //        return new MovieSenderResult { Result = true, MovieSendingEnabled = true };
        //    }
        //    return new MovieSenderResult { Result = false, MovieSendingEnabled = true };
        //}
    }
}