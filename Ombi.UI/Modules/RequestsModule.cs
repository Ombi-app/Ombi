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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses.Negotiation;
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Sonarr;
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Helpers.Analytics;
using Ombi.Helpers.Permissions;
using Ombi.Services.Interfaces;
using Ombi.Services.Notification;
using Ombi.Store;
using Ombi.UI.Models;
using Ombi.UI.Models.Admin;
using Ombi.UI.Models.Requests;
using Action = Ombi.Helpers.Analytics.Action;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Modules
{
    public class RequestsModule : BaseAuthModule
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
            ISickRageApi sickRageApi,
            ICacheProvider cache,
            IAnalytics an,
            IPlexNotificationEngine engine,
            IEmbyNotificationEngine embyEngine,
            ISecurityExtensions security,
            ISettingsService<CustomizationSettings> customSettings,
            ISettingsService<EmbySettings> embyS,
            ISettingsService<RadarrSettings> radarr,
            IRadarrApi radarrApi) : base("requests", prSettings, security)
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
            Cache = cache;
            Analytics = an;
            PlexNotificationEngine = engine;
            EmbyNotificationEngine = embyEngine;
            CustomizationSettings = customSettings;
            EmbySettings = embyS;
            Radarr = radarr;
            RadarrApi = radarrApi;

            Get["/", true] = async (x, ct) => await LoadRequests();
            Get["/movies", true] = async (x, ct) => await GetMovies();
            Get["/tvshows", true] = async (c, ct) => await GetTvShows();
            Get["/albums", true] = async (x, ct) => await GetAlbumRequests();
            Post["/delete", true] = async (x, ct) => await DeleteRequest((int)Request.Form.id);
            Post["/reportissue", true] = async (x, ct) => await ReportIssue((int)Request.Form.requestId, (IssueState)(int)Request.Form.issue, null);
            Post["/reportissuecomment", true] = async (x, ct) => await ReportIssue((int)Request.Form.requestId, IssueState.Other, (string)Request.Form.commentArea);

            Post["/clearissues", true] = async (x, ct) => await ClearIssue((int)Request.Form.Id);

            Post["/changeavailability", true] = async (x, ct) => await ChangeRequestAvailability((int)Request.Form.Id, (bool)Request.Form.Available);

            Post["/changeRootFoldertv", true] = async (x, ct) => await ChangeRootFolder(RequestType.TvShow, (int)Request.Form.requestId, (int)Request.Form.rootFolderId);
            Post["/changeRootFoldermovie", true] = async (x, ct) => await ChangeRootFolder(RequestType.Movie, (int)Request.Form.requestId, (int)Request.Form.rootFolderId);

            Get["/UpdateFilters", true] = async (x, ct) => await GetFilterAndSortSettings();
        }

        private static Logger Log = LogManager.GetCurrentClassLogger();
        private IRequestService Service { get; }
        private IAnalytics Analytics { get; }
        private INotificationService NotificationService { get; }
        private ISettingsService<PlexRequestSettings> PrSettings { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<SonarrSettings> SonarrSettings { get; }
        private ISettingsService<SickRageSettings> SickRageSettings { get; }
        private ISettingsService<CouchPotatoSettings> CpSettings { get; }
        private ISettingsService<CustomizationSettings> CustomizationSettings { get; }
        private ISettingsService<RadarrSettings> Radarr { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
        private ISonarrApi SonarrApi { get; }
        private IRadarrApi RadarrApi { get; }
        private ISickRageApi SickRageApi { get; }
        private ICouchPotatoApi CpApi { get; }
        private ICacheProvider Cache { get; }
        private INotificationEngine PlexNotificationEngine { get; }
        private INotificationEngine EmbyNotificationEngine { get; }

        private async Task<Negotiator> LoadRequests()
        {
            var settings = await PrSettings.GetSettingsAsync();
            var custom = await CustomizationSettings.GetSettingsAsync();

            return View["Index", new RequestsIndexViewModel { CustomizationSettings = custom, PlexRequestSettings = settings }];
        }

        private async Task<Response> GetMovies()
        {
            var allRequests = await Service.GetAllAsync();
            allRequests = allRequests.Where(x => x.Type == RequestType.Movie);

            var dbMovies = allRequests.ToList();

            if (Security.HasPermissions(User, Permissions.UsersCanViewOnlyOwnRequests) && !IsAdmin)
            {
                dbMovies = dbMovies.Where(x => x.UserHasRequested(Username)).ToList();
            }

            List<QualityModel> qualities = new List<QualityModel>();
            var rootFolders = new List<RootFolderModel>();

            var radarr = await Radarr.GetSettingsAsync();
            if (IsAdmin)
            {
                try
                {
                    var cpSettings = await CpSettings.GetSettingsAsync();
                    if (cpSettings.Enabled)
                    {
                        try
                        {
                            var result = await Cache.GetOrSetAsync(CacheKeys.CouchPotatoQualityProfiles, async () =>
                            {
                                return
                                    await Task.Run(() => CpApi.GetProfiles(cpSettings.FullUri, cpSettings.ApiKey))
                                        .ConfigureAwait(false);
                            });
                            if (result != null)
                            {
                                qualities =
                                    result.list.Select(x => new QualityModel { Id = x._id, Name = x.label }).ToList();
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Info(e);
                        }
                    }
                    if (radarr.Enabled)
                    {
                        var rootFoldersResult = await Cache.GetOrSetAsync(CacheKeys.RadarrRootFolders, async () =>
                        {
                            return await Task.Run(() => RadarrApi.GetRootFolders(radarr.ApiKey, radarr.FullUri));
                        });

                        rootFolders =
                            rootFoldersResult.Select(
                                    x => new RootFolderModel { Id = x.id.ToString(), Path = x.path, FreeSpace = x.freespace })
                                .ToList();

                        var result = await Cache.GetOrSetAsync(CacheKeys.RadarrQualityProfiles, async () =>
                        {
                            return await Task.Run(() => RadarrApi.GetProfiles(radarr.ApiKey, radarr.FullUri));
                        });
                        qualities = result.Select(x => new QualityModel { Id = x.id.ToString(), Name = x.name }).ToList();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }


            var canManageRequest = Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ManageRequests);
            var allowViewUsers = Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ViewUsers);

            var viewModel = dbMovies.Select(movie => new RequestViewModel
            {
                ProviderId = movie.ProviderId,
                Type = movie.Type,
                Status = movie.Status,
                ImdbId = movie.ImdbId,
                Id = movie.Id,
                PosterPath = movie.PosterPath,
                ReleaseDate = movie.ReleaseDate,
                ReleaseDateTicks = movie.ReleaseDate.Ticks,
                RequestedDate = movie.RequestedDate,
                Released = DateTime.Now > movie.ReleaseDate,
                RequestedDateTicks = DateTimeHelper.OffsetUTCDateTime(movie.RequestedDate, DateTimeOffset).Ticks,
                Approved = movie.Available || movie.Approved,
                Title = movie.Title,
                Overview = movie.Overview,
                RequestedUsers = canManageRequest || allowViewUsers ? movie.AllUsers.ToArray() : new string[] { },
                ReleaseYear = movie.ReleaseDate.Year.ToString(),
                Available = movie.Available,
                Admin = canManageRequest,
                IssueId = movie.IssueId,
                Denied = movie.Denied,
                DeniedReason = movie.DeniedReason,
                Qualities = qualities.ToArray(),
                HasRootFolders = rootFolders.Any(),
                RootFolders = rootFolders.ToArray(),
                CurrentRootPath = radarr.Enabled ? GetRootPath(movie.RootFolderSelected, radarr).Result : null
            }).ToList();

            return Response.AsJson(viewModel);
        }

        private async Task<Response> GetTvShows()
        {
            var requests = await Service.GetAllAsync();
            requests = requests.Where(x => x.Type == RequestType.TvShow);

            var dbTv = requests;
            if (Security.HasPermissions(User, Permissions.UsersCanViewOnlyOwnRequests) && !IsAdmin)
            {
                dbTv = dbTv.Where(x => x.UserHasRequested(Username)).ToList();
            }

            IEnumerable<QualityModel> qualities = new List<QualityModel>();
            IEnumerable<RootFolderModel> rootFolders = new List<RootFolderModel>();

            var sonarrSettings = await SonarrSettings.GetSettingsAsync();
            if (IsAdmin)
            {
                try
                {
                    if (sonarrSettings.Enabled)
                    {
                        var result = await Cache.GetOrSetAsync(CacheKeys.SonarrQualityProfiles, async () =>
                        {
                            return await Task.Run(() => SonarrApi.GetProfiles(sonarrSettings.ApiKey, sonarrSettings.FullUri));
                        });
                        qualities = result.Select(x => new QualityModel { Id = x.id.ToString(), Name = x.name }).ToList();


                        var rootFoldersResult = await Cache.GetOrSetAsync(CacheKeys.SonarrRootFolders, async () =>
                        {
                            return await Task.Run(() => SonarrApi.GetRootFolders(sonarrSettings.ApiKey, sonarrSettings.FullUri));
                        });

                        rootFolders = rootFoldersResult.Select(x => new RootFolderModel { Id = x.id.ToString(), Path = x.path, FreeSpace = x.freespace }).ToList();
                    }
                    else
                    {
                        var sickRageSettings = await SickRageSettings.GetSettingsAsync();
                        if (sickRageSettings.Enabled)
                        {
                            qualities = sickRageSettings.Qualities.Select(x => new QualityModel { Id = x.Key, Name = x.Value }).ToList();
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Info(e);
                }

            }



            var canManageRequest = Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ManageRequests);
            var allowViewUsers = Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ViewUsers);

            var viewModel = dbTv.Select(tv => new RequestViewModel
            {
                ProviderId = tv.ProviderId,
                Type = tv.Type,
                Status = tv.Status,
                ImdbId = tv.ImdbId,
                Id = tv.Id,
                PosterPath = tv.PosterPath?.Contains("http:") ?? false ? tv.PosterPath?.Replace("http:", "https:") : tv.PosterPath ?? string.Empty, // We make the poster path https on request, but this is just incase
                ReleaseDate = tv.ReleaseDate,
                ReleaseDateTicks = tv.ReleaseDate.Ticks,
                RequestedDate = tv.RequestedDate,
                RequestedDateTicks = DateTimeHelper.OffsetUTCDateTime(tv.RequestedDate, DateTimeOffset).Ticks,
                Released = DateTime.Now > tv.ReleaseDate,
                Approved = tv.Available || tv.Approved,
                Title = tv.Title,
                Overview = tv.Overview,
                RequestedUsers = canManageRequest || allowViewUsers ? tv.AllUsers.ToArray() : new string[] { },
                ReleaseYear = tv.ReleaseDate.Year.ToString(),
                Available = tv.Available,
                Admin = canManageRequest,
                IssueId = tv.IssueId,
                Denied = tv.Denied,
                DeniedReason = tv.DeniedReason,
                TvSeriesRequestType = tv.SeasonsRequested,
                Qualities = qualities.ToArray(),
                Episodes = tv.Episodes.ToArray(),
                RootFolders = rootFolders.ToArray(),
                HasRootFolders = rootFolders.Any(),
                CurrentRootPath = sonarrSettings.Enabled ? GetRootPath(tv.RootFolderSelected, sonarrSettings).Result : null
            }).ToList();

            return Response.AsJson(viewModel);
        }

        private async Task<string> GetRootPath(int pathId, SonarrSettings sonarrSettings)
        {
            var rootFoldersResult = await Cache.GetOrSetAsync(CacheKeys.SonarrRootFolders, async () =>
            {
                return await Task.Run(() => SonarrApi.GetRootFolders(sonarrSettings.ApiKey, sonarrSettings.FullUri));
            });

            foreach (var r in rootFoldersResult.Where(r => r.id == pathId))
            {
                return r.path;
            }

            int outRoot;
            var defaultPath = int.TryParse(sonarrSettings.RootPath, out outRoot);

            if (defaultPath)
            {
                // Return default path
                return rootFoldersResult.FirstOrDefault(x => x.id.Equals(outRoot))?.path ?? string.Empty;
            }
            else
            {
                return rootFoldersResult.FirstOrDefault()?.path ?? string.Empty;
            }
        }

        private async Task<string> GetRootPath(int pathId, RadarrSettings radarrSettings)
        {
            var rootFoldersResult = await Cache.GetOrSetAsync(CacheKeys.RadarrRootFolders, async () =>
            {
                return await Task.Run(() => RadarrApi.GetRootFolders(radarrSettings.ApiKey, radarrSettings.FullUri));
            });

            foreach (var r in rootFoldersResult.Where(r => r.id == pathId))
            {
                return r.path;
            }

            int outRoot;
            var defaultPath = int.TryParse(radarrSettings.RootPath, out outRoot);

            if (defaultPath)
            {
                // Return default path
                return rootFoldersResult.FirstOrDefault(x => x.id.Equals(outRoot))?.path ?? string.Empty;
            }
            else
            {
                return rootFoldersResult.FirstOrDefault()?.path ?? string.Empty;
            }
        }

        private async Task<Response> GetAlbumRequests()
        {
            var dbAlbum = await Service.GetAllAsync();
            dbAlbum = dbAlbum.Where(x => x.Type == RequestType.Album);
            if (Security.HasPermissions(User, Permissions.UsersCanViewOnlyOwnRequests) && !IsAdmin)
            {
                dbAlbum = dbAlbum.Where(x => x.UserHasRequested(Username));
            }
            var canManageRequest = Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ManageRequests);
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
                    ReleaseDate = album.ReleaseDate,
                    ReleaseDateTicks = album.ReleaseDate.Ticks,
                    RequestedDate = album.RequestedDate,
                    RequestedDateTicks = DateTimeHelper.OffsetUTCDateTime(album.RequestedDate, DateTimeOffset).Ticks,
                    Released = DateTime.Now > album.ReleaseDate,
                    Approved = album.Available || album.Approved,
                    Title = album.Title,
                    Overview = album.Overview,
                    RequestedUsers = canManageRequest ? album.AllUsers.ToArray() : new string[] { },
                    ReleaseYear = album.ReleaseDate.Year.ToString(),
                    Available = album.Available,
                    Admin = canManageRequest,
                    IssueId = album.IssueId,
                    Denied = album.Denied,
                    DeniedReason = album.DeniedReason,
                    TvSeriesRequestType = album.SeasonsRequested,
                    MusicBrainzId = album.MusicBrainzId,
                    ArtistName = album.ArtistName

                };
            }).ToList();

            return Response.AsJson(viewModel);
        }

        private async Task<Response> DeleteRequest(int requestid)
        {
            if (!Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ManageRequests))
            {
                return Response.AsJson(new JsonResponseModel { Result = true });
            }


            Analytics.TrackEventAsync(Category.Requests, Action.Delete, "Delete Request", Username, CookieHelper.GetAnalyticClientId(Cookies));

            var currentEntity = await Service.GetAsync(requestid);
            await Service.DeleteRequestAsync(currentEntity);
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
        private async Task<Response> ReportIssue(int requestId, IssueState issue, string comment)
        {
            if (!Security.HasPermissions(User, Permissions.ReportIssue))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Sorry, you do not have the correct permissions to report an issue." });
            }
            var originalRequest = await Service.GetAsync(requestId);
            if (originalRequest == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not add issue, please try again or contact the administrator!" });
            }
            originalRequest.Issues = issue;
            originalRequest.OtherMessage = !string.IsNullOrEmpty(comment)
                ? $"{Username} - {comment}"
                : string.Empty;


            var result = await Service.UpdateRequestAsync(originalRequest);

            var model = new NotificationModel
            {
                User = Username,
                NotificationType = NotificationType.Issue,
                Title = originalRequest.Title,
                DateTime = DateTime.Now,
                Body = issue == IssueState.Other ? comment : issue.ToString().ToCamelCaseWords(),
                ImgSrc = originalRequest.Type == RequestType.Movie ? $"https://image.tmdb.org/t/p/w300/{originalRequest.PosterPath}" : originalRequest.PosterPath
            };
            await NotificationService.Publish(model);

            return Response.AsJson(result
                ? new JsonResponseModel { Result = true }
                : new JsonResponseModel { Result = false, Message = "Could not add issue, please try again or contact the administrator!" });
        }

        private async Task<Response> ClearIssue(int requestId)
        {
            if (!Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ManageRequests))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Sorry, you do not have the correct permissions to clear an issue." });
            }

            var originalRequest = await Service.GetAsync(requestId);
            if (originalRequest == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Request does not exist to clear it!" });
            }
            originalRequest.Issues = IssueState.None;
            originalRequest.OtherMessage = string.Empty;

            var result = await Service.UpdateRequestAsync(originalRequest);
            return Response.AsJson(result
                                       ? new JsonResponseModel { Result = true }
                                       : new JsonResponseModel { Result = false, Message = "Could not clear issue, please try again or check the logs" });
        }

        private async Task<Response> ChangeRequestAvailability(int requestId, bool available)
        {
            if (!Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ManageRequests))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Sorry, you do not have the correct permissions to change a request." });
            }

            Analytics.TrackEventAsync(Category.Requests, Action.Update, available ? "Make request available" : "Make request unavailable", Username, CookieHelper.GetAnalyticClientId(Cookies));
            var originalRequest = await Service.GetAsync(requestId);
            if (originalRequest == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Request does not exist to change the availability!" });
            }

            originalRequest.Available = available;

            var result = await Service.UpdateRequestAsync(originalRequest);

            var plexSettings = await PlexSettings.GetSettingsAsync();
            if (available)
            {
                if (plexSettings.Enable)
                {
                    await
                        PlexNotificationEngine.NotifyUsers(originalRequest,
                            NotificationType.RequestAvailable);
                }

                var embySettings = await EmbySettings.GetSettingsAsync();
                if (embySettings.Enable)
                {
                    await EmbyNotificationEngine.NotifyUsers(originalRequest,
                        NotificationType.RequestAvailable);
                }
            }
            return Response.AsJson(result
                                       ? new { Result = true, Available = available, Message = string.Empty }
                                       : new { Result = false, Available = false, Message = "Could not update the availability, please try again or check the logs" });
        }

        private async Task<Response> GetFilterAndSortSettings()
        {
            var s = await CustomizationSettings.GetSettingsAsync();

            var sortVal = EnumHelper<SortOptions>.GetDisplayValue((SortOptions)s.DefaultSort);
            var filterVal = EnumHelper<FilterOptions>.GetDisplayValue((FilterOptions)s.DefaultFilter);

            var vm = new
            {
                DefaultSort = sortVal,
                DefaultFilter = filterVal
            };

            return Response.AsJson(vm);
        }

        private async Task<Response> ChangeRootFolder(RequestType type, int id, int rootFolderId)
        {
            var rootFolders = new List<SonarrRootFolder>();
            if (type == RequestType.TvShow)
            {
                // Get all root folders
                var settings = await SonarrSettings.GetSettingsAsync();
                rootFolders = SonarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
            }
            else
            {

                var settings = await Radarr.GetSettingsAsync();
                rootFolders = RadarrApi.GetRootFolders(settings.ApiKey, settings.FullUri);
            }

            // Get Request
            var allRequests = await Service.GetAllAsync();
            var request = allRequests.FirstOrDefault(x => x.Id == id);

            if (request == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false });
            }

            foreach (var folder in rootFolders)
            {
                if (folder.id.Equals(rootFolderId))
                {
                    request.RootFolderSelected = folder.id;
                    break;
                }
            }

            await Service.UpdateRequestAsync(request);

            return Response.AsJson(new JsonResponseModel { Result = true });
        }
    }
}
