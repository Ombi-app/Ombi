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
using NLog;
using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.SickRage;
using PlexRequests.Api.Models.Sonarr;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Store;
using System.Linq;

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

        public SonarrAddSeries SendToSonarr(SonarrSettings sonarrSettings, RequestedModel model)
        {
            return SendToSonarr(sonarrSettings, model, string.Empty);
        }

        public SonarrAddSeries SendToSonarr(SonarrSettings sonarrSettings, RequestedModel model, string qualityId)
        {
            int qualityProfile;
            
            if (!string.IsNullOrEmpty(qualityId) || !int.TryParse(qualityId, out qualityProfile)) // try to parse the passed in quality, otherwise use the settings default quality
            {
                int.TryParse(sonarrSettings.QualityProfile, out qualityProfile);
            }
            
            var result = SonarrApi.AddSeries(model.ProviderId, model.Title, qualityProfile,
                sonarrSettings.SeasonFolders, sonarrSettings.RootPath, model.SeasonCount, model.SeasonList, sonarrSettings.ApiKey,
                sonarrSettings.FullUri);

            Log.Trace("Sonarr Add Result: ");
            Log.Trace(result.DumpJson());

            return result;
        }

        public SickRageTvAdd SendToSickRage(SickRageSettings sickRageSettings, RequestedModel model)
        {
            return SendToSickRage(sickRageSettings, model, sickRageSettings.QualityProfile);
        }

        public SickRageTvAdd SendToSickRage(SickRageSettings sickRageSettings, RequestedModel model, string qualityId)
        {
            if (!sickRageSettings.Qualities.Any(x => x.Key == qualityId))
            {
                qualityId = sickRageSettings.QualityProfile;
            }

            var apiResult = SickrageApi.AddSeries(model.ProviderId, model.SeasonCount, model.SeasonList, qualityId,
                           sickRageSettings.ApiKey, sickRageSettings.FullUri);

            var result = apiResult.Result;
            Log.Trace("SickRage Add Result: ");
            Log.Trace(result.DumpJson());

            return result;
        }
    }
}