#region Copyright

// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: UserRequestLimitResetter.cs
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

using NLog;
using PlexRequests.Api;
using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services.Interfaces;
using PlexRequests.Store;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;

using Quartz;

namespace PlexRequests.Services.Jobs
{
    public class FaultQueueHandler : IJob
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public FaultQueueHandler(IJobRecord record, IRepository<RequestQueue> repo, ISonarrApi sonarrApi,
            ISickRageApi srApi,
            ISettingsService<SonarrSettings> sonarrSettings, ISettingsService<SickRageSettings> srSettings)
        {
            Record = record;
            Repo = repo;
            SonarrApi = sonarrApi;
            SrApi = srApi;
            SickrageSettings = srSettings;
            SonarrSettings = sonarrSettings;
        }

        private IRepository<RequestQueue> Repo { get; }
        private IJobRecord Record { get; }
        private ISonarrApi SonarrApi { get; }
        private ISickRageApi SrApi { get; }
        private ISettingsService<SonarrSettings> SonarrSettings { get; set; }
        private ISettingsService<SickRageSettings> SickrageSettings { get; set; }


        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var faultedRequests = Repo.GetAll().ToList();


                var missingInfo = faultedRequests.Where(x => x.FaultType == FaultType.MissingInformation).ToList();
                ProcessMissingInformation(missingInfo);

                var transientErrors = faultedRequests.Where(x => x.FaultType == FaultType.RequestFault).ToList();
                ProcessTransientErrors(transientErrors);

            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                Record.Record(JobNames.RequestLimitReset);
            }
        }


        private void ProcessMissingInformation(List<RequestQueue> requests)
        {
            var sonarrSettings = SonarrSettings.GetSettings();
            var sickrageSettings = SickrageSettings.GetSettings();

            if (!requests.Any())
            {
                return;
            }
            var tv = requests.Where(x => x.Type == RequestType.TvShow);

            // TV
            var tvApi = new TvMazeApi();
            foreach (var t in tv)
            {
                var providerId = int.Parse(t.PrimaryIdentifier);
                var showInfo = tvApi.ShowLookupByTheTvDbId(providerId);

                if (showInfo.externals?.thetvdb != null)
                {
                    // We now have the info
                    var tvModel = ByteConverterHelper.ReturnObject<RequestedModel>(t.Content);
                    tvModel.ProviderId = showInfo.externals.thetvdb.Value;
                    var result = ProcessTvShow(tvModel, sonarrSettings, sickrageSettings);

                    if (!result)
                    {
                        // we now have the info but couldn't add it, so add it back into the queue but with a different fault
                        t.Content = ByteConverterHelper.ReturnBytes(tvModel);
                        t.FaultType = FaultType.RequestFault;
                        t.LastRetry = DateTime.UtcNow;
                        Repo.Update(t);
                    }
                    else
                    {
                        // Successful, remove from the fault queue
                        Repo.Delete(t);
                    }
                }
            }
        }

        private bool ProcessTvShow(RequestedModel tvModel, SonarrSettings sonarr, SickRageSettings sickrage)
        {
            try
            {

                var sender = new TvSenderOld(SonarrApi, SrApi);
                if (sonarr.Enabled)
                {
                    var task = sender.SendToSonarr(sonarr, tvModel, sonarr.QualityProfile);
                    var a = task.Result;
                    if (string.IsNullOrEmpty(a?.title))
                    {
                        // Couldn't send it
                        return false;
                    }
                    return true;
                }

                if (sickrage.Enabled)
                {
                    var result = sender.SendToSickRage(sickrage, tvModel);
                    if (result?.result != "success")
                    {
                        // Couldn't send it
                        return false;
                    }
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false; // It fails so it will get added back into the queue
            }
        }

        private void ProcessTransientErrors(List<RequestQueue> requests)
        {
            var sonarrSettings = SonarrSettings.GetSettings();
            var sickrageSettings = SickrageSettings.GetSettings();

            if (!requests.Any())
            {
                return;
            }
            var tv = requests.Where(x => x.Type == RequestType.TvShow);
            var movie = requests.Where(x => x.Type == RequestType.Movie);
            var album = requests.Where(x => x.Type == RequestType.Album);



            foreach (var t in tv)
            {
                var tvModel = ByteConverterHelper.ReturnObject<RequestedModel>(t.Content);
                var result = ProcessTvShow(tvModel, sonarrSettings, sickrageSettings);

                if (!result)
                {
                    // we now have the info but couldn't add it, so do nothing now.
                    t.LastRetry = DateTime.UtcNow;
                    Repo.Update(t);
                }
                else
                {
                    // Successful, remove from the fault queue
                    Repo.Delete(t);
                }
            }


        }
    }
}