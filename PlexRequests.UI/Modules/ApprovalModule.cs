#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ApprovalModule.cs
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
using System.Linq.Expressions;
using System.Threading.Tasks;

using Nancy;
using Nancy.Security;

using NLog;
using PlexRequests.Api;
using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.Queue;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Helpers.Permissions;
using PlexRequests.Store;
using PlexRequests.UI.Helpers;
using PlexRequests.UI.Models;
using ISecurityExtensions = PlexRequests.Core.ISecurityExtensions;

namespace PlexRequests.UI.Modules
{
    public class ApprovalModule : BaseAuthModule
    {

        public ApprovalModule(IRequestService service, ISettingsService<CouchPotatoSettings> cpService, ICouchPotatoApi cpApi, ISonarrApi sonarrApi,
            ISettingsService<SonarrSettings> sonarrSettings, ISickRageApi srApi, ISettingsService<SickRageSettings> srSettings,
            ISettingsService<HeadphonesSettings> hpSettings, IHeadphonesApi hpApi, ISettingsService<PlexRequestSettings> pr, ITransientFaultQueue faultQueue
            , ISecurityExtensions security) : base("approval", pr, security)
        {

            Before += (ctx) => Security.AdminLoginRedirect(ctx, Permissions.Administrator,Permissions.ManageRequests);

            Service = service;
            CpService = cpService;
            CpApi = cpApi;
            SonarrApi = sonarrApi;
            SonarrSettings = sonarrSettings;
            SickRageApi = srApi;
            SickRageSettings = srSettings;
            HeadphonesSettings = hpSettings;
            HeadphoneApi = hpApi;
            FaultQueue = faultQueue;

            Post["/approve", true] = async (x, ct) => await Approve((int)Request.Form.requestid, (string)Request.Form.qualityId);
            Post["/deny", true] = async (x, ct) => await DenyRequest((int)Request.Form.requestid, (string)Request.Form.reason);
            Post["/approveall", true] = async (x, ct) => await ApproveAll();
            Post["/approveallmovies", true] = async (x, ct) => await ApproveAllMovies();
            Post["/approvealltvshows", true] = async (x, ct) => await ApproveAllTVShows();
            Post["/deleteallmovies", true] = async (x, ct) => await DeleteAllMovies();
            Post["/deletealltvshows", true] = async (x, ct) => await DeleteAllTVShows();
            Post["/deleteallalbums", true] = async (x, ct) => await DeleteAllAlbums();
        }

        private IRequestService Service { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();
        private ISettingsService<SonarrSettings> SonarrSettings { get; }
        private ISettingsService<SickRageSettings> SickRageSettings { get; }
        private ISettingsService<CouchPotatoSettings> CpService { get; }
        private ISettingsService<HeadphonesSettings> HeadphonesSettings { get; }
        private ISonarrApi SonarrApi { get; }
        private ISickRageApi SickRageApi { get; }
        private ICouchPotatoApi CpApi { get; }
        private IHeadphonesApi HeadphoneApi { get; }
        private ITransientFaultQueue FaultQueue { get; }

        /// <summary>
        /// Approves the specified request identifier.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns></returns>
        private async Task<Response> Approve(int requestId, string qualityId)
        {
            Log.Info("approving request {0}", requestId);

            // Get the request from the DB
            var request = await Service.GetAsync(requestId);

            if (request == null)
            {
                Log.Warn("Tried approving a request, but the request did not exist in the database, requestId = {0}", requestId);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "There are no requests to approve. Please refresh." });
            }

            switch (request.Type)
            {
                case RequestType.Movie:
                    return await RequestMovieAndUpdateStatus(request, qualityId);
                case RequestType.TvShow:
                    return await RequestTvAndUpdateStatus(request, qualityId);
                case RequestType.Album:
                    return await RequestAlbumAndUpdateStatus(request);
                default:
                    throw new ArgumentOutOfRangeException(nameof(request));
            }
        }

        private async Task<Response> RequestTvAndUpdateStatus(RequestedModel request, string qualityId)
        {
            var sender = new TvSenderOld(SonarrApi, SickRageApi); // TODO put back

            var sonarrSettings = await SonarrSettings.GetSettingsAsync();
            if (sonarrSettings.Enabled)
            {
                Log.Trace("Sending to Sonarr");
                var result = await sender.SendToSonarr(sonarrSettings, request, qualityId);
                Log.Trace("Sonarr Result: ");
                Log.Trace(result.DumpJson());
                if (!string.IsNullOrEmpty(result.title))
                {
                    Log.Info("Sent successfully, Approving request now.");
                    request.Approved = true;
                    var requestResult = await Service.UpdateRequestAsync(request);
                    Log.Trace("Approval result: {0}", requestResult);
                    if (requestResult)
                    {
                        return Response.AsJson(new JsonResponseModel { Result = true });
                    }
                    return
                        Response.AsJson(new JsonResponseModel
                        {
                            Result = false,
                            Message = "Updated Sonarr but could not approve it in PlexRequests :("
                        });
                }
                return Response.AsJson(ValidationHelper.SendSonarrError(result.ErrorMessages));

            }

            var srSettings = await SickRageSettings.GetSettingsAsync();
            if (srSettings.Enabled)
            {
                Log.Trace("Sending to SickRage");
                var result = sender.SendToSickRage(srSettings, request, qualityId);
                Log.Trace("SickRage Result: ");
                Log.Trace(result.DumpJson());
                if (result?.result == "success")
                {
                    Log.Info("Sent successfully, Approving request now.");
                    request.Approved = true;
                    var requestResult = await Service.UpdateRequestAsync(request);
                    Log.Trace("Approval result: {0}", requestResult);
                    return Response.AsJson(requestResult
                        ? new JsonResponseModel { Result = true }
                        : new JsonResponseModel { Result = false, Message = "Updated SickRage but could not approve it in PlexRequests :(" });
                }
                return Response.AsJson(new JsonResponseModel
                {
                    Result = false,
                    Message = result?.message != null ? "<b>Message From SickRage: </b>" + result.message : "Could not add the series to SickRage"
                });
            }


            request.Approved = true;
            var res = await Service.UpdateRequestAsync(request);
            return Response.AsJson(res
                ? new JsonResponseModel { Result = true, Message = "This has been approved, but It has not been sent to Sonarr/SickRage because it has not been configured" }
                : new JsonResponseModel { Result = false, Message = "Updated SickRage but could not approve it in PlexRequests :(" });
        }

        private async Task<Response> RequestMovieAndUpdateStatus(RequestedModel request, string qualityId)
        {
            var cpSettings = await CpService.GetSettingsAsync();

            Log.Info("Adding movie to CouchPotato : {0}", request.Title);
            if (!cpSettings.Enabled)
            {
                // Approve it
                request.Approved = true;
                Log.Warn("We approved movie: {0} but could not add it to CouchPotato because it has not been setup", request.Title);

                // Update the record
                var inserted = await Service.UpdateRequestAsync(request);
                return Response.AsJson(inserted
                    ? new JsonResponseModel { Result = true, Message = "This has been approved, but It has not been sent to CouchPotato because it has not been configured." }
                    : new JsonResponseModel
                    {
                        Result = false,
                        Message = "We could not approve this request. Please try again or check the logs."
                    });
            }

            var result = CpApi.AddMovie(request.ImdbId, cpSettings.ApiKey, request.Title, cpSettings.FullUri, string.IsNullOrEmpty(qualityId) ? cpSettings.ProfileId : qualityId);
            Log.Trace("Adding movie to CP result {0}", result);
            if (result)
            {
                // Approve it
                request.Approved = true;

                // Update the record
                var inserted = await Service.UpdateRequestAsync(request);

                return Response.AsJson(inserted
                    ? new JsonResponseModel { Result = true }
                    : new JsonResponseModel
                    {
                        Result = false,
                        Message = "We could not approve this request. Please try again or check the logs."
                    });
            }
            return
                Response.AsJson(
                    new
                    {
                        Result = false,
                        Message =
                            "Something went wrong adding the movie to CouchPotato! Please check your settings."
                    });
        }

        private async Task<Response> RequestAlbumAndUpdateStatus(RequestedModel request)
        {
            var hpSettings = await HeadphonesSettings.GetSettingsAsync();
            Log.Info("Adding album to Headphones : {0}", request.Title);
            if (!hpSettings.Enabled)
            {
                // Approve it
                request.Approved = true;
                Log.Warn("We approved Album: {0} but could not add it to Headphones because it has not been setup", request.Title);

                // Update the record
                var inserted = await Service.UpdateRequestAsync(request);
                return Response.AsJson(inserted
                    ? new JsonResponseModel { Result = true, Message = "This has been approved, but It has not been sent to Headphones because it has not been configured." }
                    : new JsonResponseModel
                    {
                        Result = false,
                        Message = "We could not approve this request. Please try again or check the logs."
                    });
            }

            var sender = new HeadphonesSender(HeadphoneApi, hpSettings, Service);
            var result = sender.AddAlbum(request);


            return Response.AsJson(new JsonResponseModel { Result = true, Message = "We have sent the approval to Headphones for processing, This can take a few minutes." });
        }

        private async Task<Response> ApproveAllMovies()
        {

            var requests = await Service.GetAllAsync();
            requests = requests.Where(x => x.CanApprove && x.Type == RequestType.Movie);
            var requestedModels = requests as RequestedModel[] ?? requests.ToArray();
            if (!requestedModels.Any())
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "There are no movie requests to approve. Please refresh." });
            }

            try
            {
                return await UpdateRequestsAsync(requestedModels);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Something bad happened, please check the logs!" });
            }
        }

        private async Task<Response> DeleteAllMovies()
        {

            var requests = await Service.GetAllAsync();
            requests = requests.Where(x => x.Type == RequestType.Movie);
            var requestedModels = requests as RequestedModel[] ?? requests.ToArray();
            if (!requestedModels.Any())
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "There are no movie requests to delete. Please refresh." });
            }

            try
            {
                return await DeleteRequestsAsync(requestedModels);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Something bad happened, please check the logs!" });
            }
        }

        private async Task<Response> DeleteAllAlbums()
        {

            var requests = await Service.GetAllAsync();
            requests = requests.Where(x => x.Type == RequestType.Album);
            var requestedModels = requests as RequestedModel[] ?? requests.ToArray();
            if (!requestedModels.Any())
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "There are no album requests to delete. Please refresh." });
            }

            try
            {
                return await DeleteRequestsAsync(requestedModels);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Something bad happened, please check the logs!" });
            }
        }

        private async Task<Response> ApproveAllTVShows()
        {
            var requests = await Service.GetAllAsync();
            requests = requests.Where(x => x.CanApprove && x.Type == RequestType.TvShow);
            var requestedModels = requests as RequestedModel[] ?? requests.ToArray();
            if (!requestedModels.Any())
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "There are no tv show requests to approve. Please refresh." });
            }

            try
            {
                return await UpdateRequestsAsync(requestedModels);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Something bad happened, please check the logs!" });
            }
        }

        private async Task<Response> DeleteAllTVShows()
        {

            var requests = await Service.GetAllAsync();
            requests = requests.Where(x => x.Type == RequestType.TvShow);
            var requestedModels = requests as RequestedModel[] ?? requests.ToArray();
            if (!requestedModels.Any())
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "There are no tv show requests to delete. Please refresh." });
            }

            try
            {
                return await DeleteRequestsAsync(requestedModels);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Something bad happened, please check the logs!" });
            }
        }

        /// <summary>
        /// Approves all.
        /// </summary>
        /// <returns></returns>
        private async Task<Response> ApproveAll()
        {
            var requests = await Service.GetAllAsync();
            requests = requests.Where(x => x.CanApprove);
            var requestedModels = requests as RequestedModel[] ?? requests.ToArray();
            if (!requestedModels.Any())
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "There are no requests to approve. Please refresh." });
            }

            try
            {
                return await UpdateRequestsAsync(requestedModels);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Something bad happened, please check the logs!" });
            }

        }

        private async Task<Response> DeleteRequestsAsync(IEnumerable<RequestedModel> requestedModels)
        {
            try
            {
                var result = await Service.BatchDeleteAsync(requestedModels);
                return Response.AsJson(result
                    ? new JsonResponseModel { Result = true }
                    : new JsonResponseModel { Result = false, Message = "We could not delete all of the requests. Please try again or check the logs." });
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Something bad happened, please check the logs!" });
            }
        }

        private async Task<Response> UpdateRequestsAsync(RequestedModel[] requestedModels)
        {
            var cpSettings = await CpService.GetSettingsAsync();
            var updatedRequests = new List<RequestedModel>();
            foreach (var r in requestedModels)
            {
                if (r.Type == RequestType.Movie)
                {
                    if (cpSettings.Enabled)
                    {
                        var res = SendMovie(cpSettings, r, CpApi);
                        if (res)
                        {
                            r.Approved = true;
                            updatedRequests.Add(r);
                        }
                        else
                        {
                            Log.Error("Could not approve and send the movie {0} to couch potato!", r.Title);
                        }
                    }
                    else
                    {
                        r.Approved = true;
                        updatedRequests.Add(r);
                    }
                }
                if (r.Type == RequestType.TvShow)
                {
                    var sender = new TvSenderOld(SonarrApi, SickRageApi); // TODO put back
                    var sr = await SickRageSettings.GetSettingsAsync();
                    var sonarr = await SonarrSettings.GetSettingsAsync();
                    if (sr.Enabled)
                    {
                        var res = sender.SendToSickRage(sr, r);
                        if (res?.result == "success")
                        {
                            r.Approved = true;
                            updatedRequests.Add(r);
                        }
                        else
                        {
                            Log.Error("Could not approve and send the TV {0} to SickRage!", r.Title);
                            Log.Error("SickRage Message: {0}", res?.message);
                        }
                    }

                    else if (sonarr.Enabled)
                    {
                        var res = await sender.SendToSonarr(sonarr, r);
                        if (!string.IsNullOrEmpty(res?.title))
                        {
                            r.Approved = true;
                            updatedRequests.Add(r);
                        }
                        else
                        {
                            Log.Error("Could not approve and send the TV {0} to Sonarr!", r.Title);
                            res?.ErrorMessages?.ForEach(x => Log.Error("Error messages: {0}", x));
                        }
                    }
                    else
                    {
                        r.Approved = true;
                        updatedRequests.Add(r);
                    }
                }
            }
            try
            {
                var result = await Service.BatchUpdateAsync(updatedRequests);
                return Response.AsJson(result
                    ? new JsonResponseModel { Result = true }
                    : new JsonResponseModel { Result = false, Message = "We could not approve all of the requests. Please try again or check the logs." });
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Something bad happened, please check the logs!" });
            }
        }

        private async Task<Response> DenyRequest(int requestId, string reason)
        {
            // Get the request from the DB
            var request = await Service.GetAsync(requestId);

            // Deny it
            request.Denied = true;
            request.DeniedReason = reason;

            // Update the new value
            var result = await Service.UpdateRequestAsync(request);

            return result
                ? Response.AsJson(new JsonResponseModel { Result = true, Message = "Request has been denied" })
                : Response.AsJson(new JsonResponseModel { Result = false, Message = "An error happened, could not update the DB" });

        }

        private bool SendMovie(CouchPotatoSettings settings, RequestedModel r, ICouchPotatoApi cp)
        {
            Log.Info("Adding movie to CP : {0}", r.Title);
            var result = cp.AddMovie(r.ImdbId, settings.ApiKey, r.Title, settings.FullUri, settings.ProfileId);
            Log.Trace("Adding movie to CP result {0}", result);
            return result;
        }
    }
}