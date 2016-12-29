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
using Ombi.Api;
using Ombi.Api.Interfaces;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Helpers.Permissions;
using Ombi.Services.Interfaces;
using Ombi.Store;
using Ombi.Store.Models;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Services.Jobs
{
    public class FaultQueueHandler : IJob
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public FaultQueueHandler(IJobRecord record, IRepository<RequestQueue> repo, ISonarrApi sonarrApi,
            ISickRageApi srApi, ISettingsService<SonarrSettings> sonarrSettings, ISettingsService<SickRageSettings> srSettings,
            ICouchPotatoApi cpApi, ISettingsService<CouchPotatoSettings> cpsettings, IRequestService requestService,
            ISettingsService<HeadphonesSettings> hpSettings, IHeadphonesApi headphonesApi, ISettingsService<PlexRequestSettings> prSettings,
            ISecurityExtensions security, IMovieSender movieSender)
        {
            Record = record;
            Repo = repo;
            SonarrApi = sonarrApi;
            SrApi = srApi;
            CpApi = cpApi;
            HpApi = headphonesApi;

            RequestService = requestService;

            SickrageSettings = srSettings;
            SonarrSettings = sonarrSettings;
            CpSettings = cpsettings;
            HeadphoneSettings = hpSettings;
            Security = security;
            PrSettings = prSettings.GetSettings();
            MovieSender = movieSender;
        }

        private IMovieSender MovieSender { get; }
        private IRepository<RequestQueue> Repo { get; }
        private IJobRecord Record { get; }
        private ISonarrApi SonarrApi { get; }
        private ISickRageApi SrApi { get; }
        private ICouchPotatoApi CpApi { get; }
        private IHeadphonesApi HpApi { get; }
        private IRequestService RequestService { get; }
        private PlexRequestSettings PrSettings { get; }
        private ISettingsService<SonarrSettings> SonarrSettings { get; }
        private ISettingsService<SickRageSettings> SickrageSettings { get; }
        private ISettingsService<CouchPotatoSettings> CpSettings { get; }
        private ISettingsService<HeadphonesSettings> HeadphoneSettings { get; }
        private ISecurityExtensions Security { get; }

        public void Execute(IJobExecutionContext context)
        {

            Record.SetRunning(true, JobNames.CpCacher);
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
                Record.Record(JobNames.FaultQueueHandler);
                Record.SetRunning(false, JobNames.CpCacher);
            }
        }


        private void ProcessMissingInformation(List<RequestQueue> requests)
        {
            if (!requests.Any())
            {
                return;
            }

            var sonarrSettings = SonarrSettings.GetSettings();
            var sickrageSettings = SickrageSettings.GetSettings();

            var tv = requests.Where(x => x.Type == RequestType.TvShow);

            // TV
            var tvApi = new TvMazeApi();
            foreach (var t in tv)
            {
                var providerId = int.Parse(t.PrimaryIdentifier);
                var showInfo = tvApi.ShowLookup(providerId);

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

                    // Approve it
                    tvModel.Approved = true;
                    RequestService.UpdateRequest(tvModel);
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

        private bool ProcessMovies(RequestedModel model)
        {
            try
            {
                var result = MovieSender.Send(model).Result;
                if (result.Result)
                {
                    // Approve it now
                    model.Approved = true;
                    RequestService.UpdateRequest(model);
                    return true;
                };
                
                return false;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false; // It fails so it will get added back into the queue
            }
        }

        private bool ProcessAlbums(RequestedModel model, HeadphonesSettings hp)
        {
            try
            {
                if (hp.Enabled)
                {
                    var sender = new HeadphonesSender(HpApi, hp, RequestService);
                    var result = sender.AddAlbum(model).Result;

                    if (result)
                    {

                        if (ShouldAutoApprove(model.Type, PrSettings, model.RequestedUsers))
                            // Approve it now
                            model.Approved = true;
                        RequestService.UpdateRequest(model);
                    };

                    return result;
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
            var cpSettings = CpSettings.GetSettings();
            var hpSettings = HeadphoneSettings.GetSettings();

            if (!requests.Any())
            {
                return;
            }

            foreach (var request in requests)
            {
                var model = ByteConverterHelper.ReturnObject<RequestedModel>(request.Content);
                bool result;
                switch (request.Type)
                {
                    case RequestType.Movie:
                        result = ProcessMovies(model);
                        break;
                    case RequestType.TvShow:
                        result = ProcessTvShow(model, sonarrSettings, sickrageSettings);
                        break;
                    case RequestType.Album:
                        result = ProcessAlbums(model, hpSettings);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (!result)
                {
                    // we now have the info but couldn't add it, so do nothing now.
                    request.LastRetry = DateTime.UtcNow;
                    Repo.Update(request);
                }
                else
                {
                    // Successful, remove from the fault queue
                    Repo.Delete(request);
                }
            }
        }

        public bool ShouldAutoApprove(RequestType requestType, PlexRequestSettings prSettings, List<string> username)
        {
            foreach (var user in username)
            {
                var admin = Security.HasPermissions(user, Permissions.Administrator);
                // if the user is an admin or they are whitelisted, they go ahead and allow auto-approval
                if (admin) return true;

                // check by request type if the category requires approval or not
                switch (requestType)
                {
                    case RequestType.Movie:
                        return Security.HasPermissions(user, Permissions.AutoApproveMovie);
                    case RequestType.TvShow:
                        return Security.HasPermissions(user, Permissions.AutoApproveTv);
                    case RequestType.Album:
                        return Security.HasPermissions(user, Permissions.AutoApproveAlbum);
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}