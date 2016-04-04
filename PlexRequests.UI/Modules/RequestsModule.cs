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

using System;
using System.Linq;

using Humanizer;

using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.Security;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Notification;
using PlexRequests.Store;
using PlexRequests.UI.Models;
using PlexRequests.Helpers;
using System.Collections.Generic;
using PlexRequests.Api.Interfaces;
using System.Threading.Tasks;

namespace PlexRequests.UI.Modules
{
    public class RequestsModule : BaseModule
    {
        public RequestsModule(
            IRequestService service,
            ISettingsService<PlexRequestSettings> prSettings,
            ISettingsService<PlexSettings> plex,
            INotificationService notify,
            ISettingsService<SonarrSettings> sonarrSettings,
            ISettingsService<SickRageSettings> sickRageSettings,
            ISettingsService<CouchPotatoSettings> cpSettings,
            ICouchPotatoApi cpApi,
            ISonarrApi sonarrApi,
            ISickRageApi sickRageApi) : base("requests")
        {
            Service = service;
            PrSettings = prSettings;
            PlexSettings = plex;
            NotificationService = notify;
            SonarrSettings = sonarrSettings;
            SickRageSettings = sickRageSettings;
            CpSettings = cpSettings;
            SonarrApi = sonarrApi;
            SickRageApi = sickRageApi;
            CpApi = cpApi;

            Get["/"] = _ => LoadRequests();
            Get["/movies"] = _ => GetMovies();
            Get["/tvshows"] = _ => GetTvShows();
            Get["/albums"] = _ => GetAlbumRequests();
            Post["/delete"] = _ => DeleteRequest((int)Request.Form.id);
            Post["/reportissue"] = _ => ReportIssue((int)Request.Form.requestId, (IssueState)(int)Request.Form.issue, null);
            Post["/reportissuecomment"] = _ => ReportIssue((int)Request.Form.requestId, IssueState.Other, (string)Request.Form.commentArea);

            Post["/clearissues"] = _ => ClearIssue((int)Request.Form.Id);

            Post["/changeavailability"] = _ => ChangeRequestAvailability((int)Request.Form.Id, (bool)Request.Form.Available);
            Post["/addnote"] = _ => AddNote((int)Request.Form.requestId, (string)Request.Form.noteArea);
        }

        private IRequestService Service { get; }
        private INotificationService NotificationService { get; }
        private ISettingsService<PlexRequestSettings> PrSettings { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<SonarrSettings> SonarrSettings { get; }
        private ISettingsService<SickRageSettings> SickRageSettings { get; }
        private ISettingsService<CouchPotatoSettings> CpSettings { get; }
        private ISonarrApi SonarrApi { get; }
        private ISickRageApi SickRageApi { get; }
        private ICouchPotatoApi CpApi { get; }

        private Negotiator LoadRequests()
        {
            var settings = PrSettings.GetSettings();
            return View["Index", settings];
        }

        private Response GetMovies() // TODO: async await the API calls
        {
            var settings = PrSettings.GetSettings();
            var isAdmin = Context.CurrentUser.IsAuthenticated();

            List<Task> taskList = new List<Task>();

            List<RequestedModel> dbMovies = new List<RequestedModel>();
            taskList.Add(Task.Factory.StartNew(() =>
            {
                return Service.GetAll().Where(x => x.Type == RequestType.Movie);

            }).ContinueWith((t) =>
            {
                dbMovies = t.Result.ToList();

                if (settings.UsersCanViewOnlyOwnRequests && !isAdmin)
                {
                    dbMovies = dbMovies.Where(x => x.UserHasRequested(Username)).ToList();
                }
            }));


            var mc = new MemoryCacheProvider();
            List<QualityModel> qualities = new List<QualityModel>();

            if (isAdmin)
            {
                var cpSettings = CpSettings.GetSettings();
                if (cpSettings.Enabled)
                {
                    taskList.Add(Task.Factory.StartNew(() =>
                    {
                        return mc.GetOrSet(CacheKeys.CouchPotatoQualityProfiles, () =>
                        {
                            return CpApi.GetProfiles(cpSettings.FullUri, cpSettings.ApiKey); // TODO: cache this!
                        });
                    }).ContinueWith((t) =>
                    {
                        qualities = t.Result.list.Select(x => new QualityModel() { Id = x._id, Name = x.label }).ToList();
                    }));
                }
            }

            Task.WaitAll(taskList.ToArray());

            var viewModel = dbMovies.Select(movie =>
            {
                return new RequestViewModel
                {
                    ProviderId = movie.ProviderId,
                    Type = movie.Type,
                    Status = movie.Status,
                    ImdbId = movie.ImdbId,
                    Id = movie.Id,
                    PosterPath = movie.PosterPath,
                    ReleaseDate = movie.ReleaseDate.Humanize(),
                    ReleaseDateTicks = movie.ReleaseDate.Ticks,
                    RequestedDate = DateTimeHelper.OffsetUTCDateTime(movie.RequestedDate, DateTimeOffset).Humanize(),
                    RequestedDateTicks = DateTimeHelper.OffsetUTCDateTime(movie.RequestedDate, DateTimeOffset).Ticks,
                    Approved = movie.Available || movie.Approved,
                    Title = movie.Title,
                    Overview = movie.Overview,
                    RequestedUsers = isAdmin ? movie.AllUsers.ToArray() : new string[] { },
                    ReleaseYear = movie.ReleaseDate.Year.ToString(),
                    Available = movie.Available,
                    Admin = isAdmin,
                    Issues = movie.Issues.Humanize(LetterCasing.Title),
                    OtherMessage = movie.OtherMessage,
                    AdminNotes = movie.AdminNote,
                    Qualities = qualities.ToArray()
                };
            }).ToList();

            return Response.AsJson(viewModel);
        }

        private Response GetTvShows() // TODO: async await the API calls
        {
            var settings = PrSettings.GetSettings();
            var isAdmin = Context.CurrentUser.IsAuthenticated();

            List<Task> taskList = new List<Task>();

            List<RequestedModel> dbTv = new List<RequestedModel>();
            taskList.Add(Task.Factory.StartNew(() =>
            {
                return Service.GetAll().Where(x => x.Type == RequestType.TvShow);

            }).ContinueWith((t) =>
            {
                dbTv = t.Result.ToList();

                if (settings.UsersCanViewOnlyOwnRequests && !isAdmin)
                {
                    dbTv = dbTv.Where(x => x.UserHasRequested(Username)).ToList();
                }
            }));

            var mc = new MemoryCacheProvider();
            List<QualityModel> qualities = new List<QualityModel>();
            if (isAdmin)
            {
                var sonarrSettings = SonarrSettings.GetSettings();
                if (sonarrSettings.Enabled)
                {
                    taskList.Add(Task.Factory.StartNew(() =>
                    {
                        return mc.GetOrSet(CacheKeys.SonarrQualityProfiles, () =>
                        {
                            return SonarrApi.GetProfiles(sonarrSettings.ApiKey, sonarrSettings.FullUri); // TODO: cache this!

                    });
                    }).ContinueWith((t) =>
                    {
                        qualities = t.Result.Select(x => new QualityModel() { Id = x.id.ToString(), Name = x.name }).ToList();
                    }));
                }
                else {
                    var sickRageSettings = SickRageSettings.GetSettings();
                    if (sickRageSettings.Enabled)
                    {
                        qualities = sickRageSettings.Qualities.Select(x => new QualityModel() { Id = x.Key, Name = x.Value }).ToList();
                    }
                }
            }

            Task.WaitAll(taskList.ToArray());

            var viewModel = dbTv.Select(tv =>
            {
                return new RequestViewModel
                {
                    ProviderId = tv.ProviderId,
                    Type = tv.Type,
                    Status = tv.Status,
                    ImdbId = tv.ImdbId,
                    Id = tv.Id,
                    PosterPath = tv.PosterPath,
                    ReleaseDate = tv.ReleaseDate.Humanize(),
                    ReleaseDateTicks = tv.ReleaseDate.Ticks,
                    RequestedDate = DateTimeHelper.OffsetUTCDateTime(tv.RequestedDate, DateTimeOffset).Humanize(),
                    RequestedDateTicks = DateTimeHelper.OffsetUTCDateTime(tv.RequestedDate, DateTimeOffset).Ticks,
                    Approved = tv.Available || tv.Approved,
                    Title = tv.Title,
                    Overview = tv.Overview,
                    RequestedUsers = isAdmin ? tv.AllUsers.ToArray() : new string[] { },
                    ReleaseYear = tv.ReleaseDate.Year.ToString(),
                    Available = tv.Available,
                    Admin = isAdmin,
                    Issues = tv.Issues.Humanize(LetterCasing.Title),
                    OtherMessage = tv.OtherMessage,
                    AdminNotes = tv.AdminNote,
                    TvSeriesRequestType = tv.SeasonsRequested,
                    Qualities = qualities.ToArray()
                };
            }).ToList();

            return Response.AsJson(viewModel);
        }

        private Response GetAlbumRequests()
        {
            var settings = PrSettings.GetSettings();
            var isAdmin = Context.CurrentUser.IsAuthenticated();
            var dbAlbum = Service.GetAll().Where(x => x.Type == RequestType.Album);
            if (settings.UsersCanViewOnlyOwnRequests && !isAdmin)
            {
                dbAlbum = dbAlbum.Where(x => x.UserHasRequested(Username));
            }

            var viewModel = dbAlbum.Select(album =>
            {
                return new RequestViewModel
                {
                    ProviderId = album.ProviderId,
                    Type = album.Type,
                    Status = album.Status,
                    ImdbId = album.ImdbId,
                    Id = album.Id,
                    PosterPath = album.PosterPath,
                    ReleaseDate = album.ReleaseDate.Humanize(),
                    ReleaseDateTicks = album.ReleaseDate.Ticks,
                    RequestedDate = DateTimeHelper.OffsetUTCDateTime(album.RequestedDate, DateTimeOffset).Humanize(),
                    RequestedDateTicks = DateTimeHelper.OffsetUTCDateTime(album.RequestedDate, DateTimeOffset).Ticks,
                    Approved = album.Available || album.Approved,
                    Title = album.Title,
                    Overview = album.Overview,
                    RequestedUsers = isAdmin ? album.AllUsers.ToArray() : new string[] { },
                    ReleaseYear = album.ReleaseDate.Year.ToString(),
                    Available = album.Available,
                    Admin = isAdmin,
                    Issues = album.Issues.Humanize(LetterCasing.Title),
                    OtherMessage = album.OtherMessage,
                    AdminNotes = album.AdminNote,
                    TvSeriesRequestType = album.SeasonsRequested,
                    MusicBrainzId = album.MusicBrainzId
                };
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
                ? $"{Username} - {comment}"
                : string.Empty;


            var result = Service.UpdateRequest(originalRequest);

            var model = new NotificationModel
            {
                User = Username,
                NotificationType = NotificationType.Issue,
                Title = originalRequest.Title,
                DateTime = DateTime.Now,
                Body = issue == IssueState.Other ? comment : issue.Humanize()
            };
            NotificationService.Publish(model);

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