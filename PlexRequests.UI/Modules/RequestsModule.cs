#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RequestsModule.cs
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

using System.Collections.Generic;
using System.Linq;

using Humanizer;

using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.Security;

using PlexRequests.Api;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class RequestsModule : BaseModule
    {

        public RequestsModule(IRequestService service, ISettingsService<PlexRequestSettings> prSettings, ISettingsService<PlexSettings> plex) : base("requests")
        {
            Service = service;
            PrSettings = prSettings;
            PlexSettings = plex;

            Get["/"] = _ => LoadRequests();
            Get["/movies"] = _ => GetMovies();
            Get["/tvshows"] = _ => GetTvShows();
            Post["/delete"] = _ => DeleteRequest((int)Request.Form.id);
            Post["/reportissue"] = _ => ReportIssue((int)Request.Form.requestId, (IssueState)(int)Request.Form.issue, null);
            Post["/reportissuecomment"] = _ => ReportIssue((int)Request.Form.requestId, IssueState.Other, (string)Request.Form.commentArea);

            Post["/clearissues"] = _ => ClearIssue((int)Request.Form.Id);

            Post["/changeavailability"] = _ => ChangeRequestAvailability((int)Request.Form.Id, (bool)Request.Form.Available);
            Post["/addnote"] = _ => AddNote((int)Request.Form.requestId, (string)Request.Form.noteArea);
        }

        private IRequestService Service { get; }
        private ISettingsService<PlexRequestSettings> PrSettings { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }

        private Negotiator LoadRequests()
        {
            var settings = PrSettings.GetSettings();
            return View["Index", settings];
        }

        private Response GetMovies()
        {
            var isAdmin = Context.CurrentUser.IsAuthenticated();
            var dbMovies = Service.GetAll().Where(x => x.Type == RequestType.Movie);
            var viewModel = dbMovies.Select(movie => new RequestViewModel
            {
                ProviderId = movie.ProviderId,
                Type = movie.Type,
                Status = movie.Status,
                ImdbId = movie.ImdbId,
                Id = movie.Id,
                PosterPath = movie.PosterPath,
                ReleaseDate = movie.ReleaseDate.Humanize(),
                RequestedDate = movie.RequestedDate.Humanize(),
                Approved = movie.Approved,
                Title = movie.Title,
                Overview = movie.Overview,
                RequestedBy = movie.RequestedBy,
                ReleaseYear = movie.ReleaseDate.Year.ToString(),
                Available = movie.Available,
                Admin = isAdmin,
                Issues = movie.Issues.Humanize(LetterCasing.Title),
                OtherMessage = movie.OtherMessage,
                AdminNotes = movie.AdminNote
            }).ToList();

            return Response.AsJson(viewModel);
        }

        private Response GetTvShows()
        {
            var isAdmin = Context.CurrentUser.IsAuthenticated();
            var dbTv = Service.GetAll().Where(x => x.Type == RequestType.TvShow);
            var viewModel = dbTv.Select(tv => new RequestViewModel
            {
                ProviderId = tv.ProviderId,
                Type = tv.Type,
                Status = tv.Status,
                ImdbId = tv.ImdbId,
                Id = tv.Id,
                PosterPath = tv.PosterPath,
                ReleaseDate = tv.ReleaseDate.Humanize(),
                RequestedDate = tv.RequestedDate.Humanize(),
                Approved = tv.Approved,
                Title = tv.Title,
                Overview = tv.Overview,
                RequestedBy = tv.RequestedBy,
                ReleaseYear = tv.ReleaseDate.Year.ToString(),
                Available = tv.Available,
                Admin = isAdmin,
                Issues = tv.Issues.Humanize(LetterCasing.Title),
                OtherMessage = tv.OtherMessage,
                AdminNotes = tv.AdminNote
            }).ToList();

            return Response.AsJson(viewModel);
        }

        private Response DeleteRequest(int requestid)
        {
            if (!Context.CurrentUser.IsAuthenticated())
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "You are not an Admin, so you cannot delete any requests." });
            }

            var currentEntity = Service.Get(requestid);
            Service.DeleteRequest(currentEntity);
            return Response.AsJson(new JsonResponseModel { Result = true });
        }

        /// <summary>
        /// Reports the issue.
        /// Comment can be null if the <c>IssueState != Other</c>
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="issue">The issue.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        private Response ReportIssue(int requestId, IssueState issue, string comment)
        {
            var originalRequest = Service.Get(requestId);
            if (originalRequest == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not add issue, please try again or contact the administrator!" });
            }
            originalRequest.Issues = issue;
            originalRequest.OtherMessage = !string.IsNullOrEmpty(comment)
                ? $"{Session[SessionKeys.UsernameKey]} - {comment}"
                : string.Empty;


            var result = Service.UpdateRequest(originalRequest);
            return Response.AsJson(result
                ? new JsonResponseModel { Result = true }
                : new JsonResponseModel { Result = false, Message = "Could not add issue, please try again or contact the administrator!" });
        }

        private Response ClearIssue(int requestId)
        {
            if (!Context.CurrentUser.IsAuthenticated())
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "You are not an Admin, so you cannot clear any issues." });
            }

            var originalRequest = Service.Get(requestId);
            if (originalRequest == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Request does not exist to clear it!" });
            }
            originalRequest.Issues = IssueState.None;
            originalRequest.OtherMessage = string.Empty;

            var result = Service.UpdateRequest(originalRequest);
            return Response.AsJson(result
                                       ? new JsonResponseModel { Result = true }
                                       : new JsonResponseModel { Result = false, Message = "Could not clear issue, please try again or check the logs" });
        }

        private Response ChangeRequestAvailability(int requestId, bool available)
        {
            var originalRequest = Service.Get(requestId);
            if (originalRequest == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Request does not exist to change the availability!" });
            }

            originalRequest.Available = available;

            var result = Service.UpdateRequest(originalRequest);
            return Response.AsJson(result
                                       ? new { Result = true, Available = available, Message = string.Empty }
                                       : new { Result = false, Available = false, Message = "Could not update the availability, please try again or check the logs" });
        }

        private Response AddNote(int requestId, string noteArea)
        {
            var originalRequest = Service.Get(requestId);
            if (originalRequest == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Request does not exist to add a note!" });
            }

            originalRequest.AdminNote = noteArea;

            var result = Service.UpdateRequest(originalRequest);
            return Response.AsJson(result
                                       ? new JsonResponseModel { Result = true }
                                       : new JsonResponseModel { Result = false, Message = "Could not update the notes, please try again or check the logs" });
        }
    }
}