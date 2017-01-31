#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: MovieSender.cs
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
using System.Threading.Tasks;
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Core.SettingModels;
using Ombi.Store;

namespace Ombi.Core
{
    public class MovieSender : IMovieSender
    {
        public MovieSender(ISettingsService<CouchPotatoSettings> cp, ISettingsService<WatcherSettings> watcher,
            ICouchPotatoApi cpApi, IWatcherApi watcherApi, IRadarrApi radarrApi, ISettingsService<RadarrSettings> radarrSettings)
        {
            CouchPotatoSettings = cp;
            WatcherSettings = watcher;
            CpApi = cpApi;
            WatcherApi = watcherApi;
            RadarrSettings = radarrSettings;
            RadarrApi = radarrApi;
        }

        private ISettingsService<CouchPotatoSettings> CouchPotatoSettings { get; }
        private ISettingsService<WatcherSettings> WatcherSettings { get; }
        private ISettingsService<RadarrSettings> RadarrSettings { get; } 
        private IRadarrApi RadarrApi { get; }
        private ICouchPotatoApi CpApi { get; }
        private IWatcherApi WatcherApi { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public async Task<MovieSenderResult> Send(RequestedModel model, string qualityId = "")
        {
            var cpSettings = await CouchPotatoSettings.GetSettingsAsync();
            var watcherSettings = await WatcherSettings.GetSettingsAsync();
            var radarrSettings = await RadarrSettings.GetSettingsAsync();

            if (cpSettings.Enabled)
            {
                return SendToCp(model, cpSettings, string.IsNullOrEmpty(qualityId) ? cpSettings.ProfileId : qualityId);
            }

            if (watcherSettings.Enabled)
            {
                return SendToWatcher(model, watcherSettings);
            }

            if (radarrSettings.Enabled)
            {
                return SendToRadarr(model, radarrSettings);
            }

            return new MovieSenderResult { Result = false, MovieSendingEnabled = false };
        }

        private MovieSenderResult SendToWatcher(RequestedModel model, WatcherSettings settings)
        {
            var result = WatcherApi.AddMovie(model.ImdbId, settings.ApiKey, settings.FullUri);

            if (result.Error)
            {
                Log.Error(result.ErrorMessage);
                return new MovieSenderResult { Result = false };
            }
            if (result.response)
            {
                return new MovieSenderResult { Result = true, MovieSendingEnabled = true };
            }
            Log.Error(result.message);
            return new MovieSenderResult { Result = false, MovieSendingEnabled = true };
        }

        private MovieSenderResult SendToCp(RequestedModel model, CouchPotatoSettings settings, string qualityId)
        {
            var result = CpApi.AddMovie(model.ImdbId, settings.ApiKey, model.Title, settings.FullUri, qualityId);
            return new MovieSenderResult { Result = result, MovieSendingEnabled = true };
        }

        private MovieSenderResult SendToRadarr(RequestedModel model, RadarrSettings settings)
        {
            var qualityProfile = 0;
            int.TryParse(settings.QualityProfile, out qualityProfile);
            var result = RadarrApi.AddMovie(model.ProviderId, model.Title, model.ReleaseDate.Year, qualityProfile, settings.RootPath, settings.ApiKey, settings.FullUri, true);

            if (!string.IsNullOrEmpty(result.Error?.message))
            {
                Log.Error(result.Error.message);
                return new MovieSenderResult { Result = false, Error = true};
            }
            if (!string.IsNullOrEmpty(result.title))
            {
                return new MovieSenderResult { Result = true, MovieSendingEnabled = true };
            }
            return new MovieSenderResult { Result = false, MovieSendingEnabled = true };
        }
    }
}