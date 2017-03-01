#region Copyright

//  ************************************************************************
//    Copyright (c) 2016-2017 Ombi
//    File: MusicSearchModule.cs
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
using System.Text;
using System.Threading.Tasks;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Api.Interfaces;
using Ombi.Services.Interfaces;
using Ombi.Store.Repository;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses.Negotiation;
using Ombi.Store.Models;
using Ombi.Core.Queue;
using Ombi.Store.Models.Plex;
using Ombi.Helpers.Analytics;
using Ombi.Services.Jobs;
using Ombi.Store.Models.Emby;
using Ombi.Api.Models.Music;
using Ombi.UI.Models;
using Ombi.Helpers;
using Ombi.Store;
using Ombi.Helpers.Permissions;
using Action = Ombi.Helpers.Analytics.Action;
using Ombi.Services.Notification;
using Ombi.Core.Models;

namespace Ombi.UI.Modules.Search
{
    public class MusicSearchModule : BaseSearchModule
    {
        private IMusicBrainzApi MusicBrainzApi { get; }
        private IHeadphonesApi HeadphonesApi { get; }
        private ISettingsService<HeadphonesSettings> HeadphonesService { get; }

        public MusicSearchModule(IMusicBrainzApi mbApi, IHeadphonesApi hpApi,
            ISettingsService<HeadphonesSettings> hpService,
            /* parameters needed for base class constructor */
            IPlexApi plexApi, ISettingsService<PlexRequestSettings> prSettings,
            ISettingsService<PlexSettings> plexService, ISettingsService<AuthenticationSettings> auth,
            ISecurityExtensions security, IAvailabilityChecker plexChecker, INotificationService notify,
            ISettingsService<CustomizationSettings> cus, IRequestService request, IAnalytics a,
            IRepository<UsersToNotify> u, ISettingsService<EmailNotificationSettings> email,
            IIssueService issue, IRepository<RequestLimit> rl, ITransientFaultQueue tfQueue,
            IRepository<PlexContent> content, IEmbyAvailabilityChecker embyChecker,
            IRepository<EmbyContent> embyContent, ISettingsService<EmbySettings> embySettings)
            : base(plexApi, prSettings,plexService, auth, security, plexChecker, notify, cus, request, a, u,
                  email, issue, rl, tfQueue,content, embyChecker, embyContent, embySettings)
        {
            HeadphonesApi = hpApi;
            HeadphonesService = hpService;
            MusicBrainzApi = mbApi;

            Get["music/{searchTerm}", true] = async (x, ct) => await SearchAlbum((string)x.searchTerm);
            Get["music/coverArt/{id}"] = p => GetMusicBrainzCoverArt((string)p.id);
            Post["request/album", true] = async (x, ct) => await RequestAlbum((string)Request.Form.albumId);
        }

        private string GetMusicBrainzCoverArt(string id)
        {
            var coverArt = MusicBrainzApi.GetCoverArt(id);
            var firstImage = coverArt?.images?.FirstOrDefault();
            var img = string.Empty;

            if (firstImage != null)
            {
                img = firstImage.thumbnails?.small ?? firstImage.image;
            }

            return img;
        }

        private async Task<Response> SearchAlbum(string searchTerm)
        {
            Analytics.TrackEventAsync(Category.Search, Action.Album, searchTerm, Username,
                CookieHelper.GetAnalyticClientId(Cookies));
            var apiAlbums = new List<Release>();
            await Task.Run(() => MusicBrainzApi.SearchAlbum(searchTerm)).ContinueWith((t) =>
            {
                apiAlbums = t.Result.releases ?? new List<Release>();
            });

            var allResults = await RequestService.GetAllAsync();
            allResults = allResults.Where(x => x.Type == RequestType.Album);

            var dbAlbum = allResults.ToDictionary(x => x.MusicBrainzId);

            var content = PlexContentRepository.GetAll();
            var plexAlbums = PlexChecker.GetPlexAlbums(content);

            var viewAlbum = new List<SearchMusicViewModel>();
            foreach (var a in apiAlbums)
            {
                var viewA = new SearchMusicViewModel
                {
                    Title = a.title,
                    Id = a.id,
                    Artist = a.ArtistCredit?.Select(x => x.artist?.name).FirstOrDefault(),
                    Overview = a.disambiguation,
                    ReleaseDate = a.date,
                    TrackCount = a.TrackCount,
                    ReleaseType = a.status,
                    Country = a.country
                };

                DateTime release;
                DateTimeHelper.CustomParse(a.ReleaseEvents?.FirstOrDefault()?.date, out release);
                var artist = a.ArtistCredit?.FirstOrDefault()?.artist;
                var plexAlbum = PlexChecker.GetAlbum(plexAlbums.ToArray(), a.title, release.ToString("yyyy"), artist?.name);
                if (plexAlbum != null)
                {
                    viewA.Available = true;
                    viewA.PlexUrl = plexAlbum.Url;
                }
                if (!string.IsNullOrEmpty(a.id) && dbAlbum.ContainsKey(a.id))
                {
                    var dba = dbAlbum[a.id];

                    viewA.Requested = true;
                    viewA.Approved = dba.Approved;
                    viewA.Available = dba.Available;
                }

                viewAlbum.Add(viewA);
            }
            return Response.AsJson(viewAlbum);
        }

        private async Task<Response> RequestAlbum(string releaseId)
        {
            if (Security.HasPermissions(User, Permissions.ReadOnlyUser) || !Security.HasPermissions(User, Permissions.RequestMusic))
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = "Sorry, you do not have the correct permissions to request music!"
                    });
            }

            var settings = await PrService.GetSettingsAsync();
            if (!await CheckRequestLimit(settings, RequestType.Album))
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = Resources.UI.Search_WeeklyRequestLimitAlbums
                    });
            }
            Analytics.TrackEventAsync(Category.Search, Action.Request, "Album", Username,
                CookieHelper.GetAnalyticClientId(Cookies));
            var existingRequest = await RequestService.CheckRequestAsync(releaseId);

            if (existingRequest != null)
            {
                if (!existingRequest.UserHasRequested(Username))
                {
                    existingRequest.RequestedUsers.Add(Username);
                    await RequestService.UpdateRequestAsync(existingRequest);
                }
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = true,
                        Message =
                            Security.HasPermissions(User, Permissions.UsersCanViewOnlyOwnRequests)
                                ? $"{existingRequest.Title} {Resources.UI.Search_SuccessfullyAdded}"
                                : $"{existingRequest.Title} {Resources.UI.Search_AlreadyRequested}"
                    });
            }

            var albumInfo = MusicBrainzApi.GetAlbum(releaseId);
            DateTime release;
            DateTimeHelper.CustomParse(albumInfo.ReleaseEvents?.FirstOrDefault()?.date, out release);

            var artist = albumInfo.ArtistCredits?.FirstOrDefault()?.artist;
            if (artist == null)
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = Resources.UI.Search_MusicBrainzError
                    });
            }


            var content = PlexContentRepository.GetAll();
            var albums = PlexChecker.GetPlexAlbums(content);
            var alreadyInPlex = PlexChecker.IsAlbumAvailable(albums.ToArray(), albumInfo.title, release.ToString("yyyy"),
                artist.name);

            if (alreadyInPlex)
            {
                return Response.AsJson(new JsonResponseModel
                {
                    Result = false,
                    Message = $"{albumInfo.title} {Resources.UI.Search_AlreadyInPlex}"
                });
            }

            var img = GetMusicBrainzCoverArt(albumInfo.id);

            var model = new RequestedModel
            {
                Title = albumInfo.title,
                MusicBrainzId = albumInfo.id,
                ReleaseId = releaseId,
                Overview = albumInfo.disambiguation,
                PosterPath = img,
                Type = RequestType.Album,
                ProviderId = 0,
                RequestedUsers = new List<string> { Username },
                Status = albumInfo.status,
                Issues = IssueState.None,
                RequestedDate = DateTime.UtcNow,
                ReleaseDate = release,
                ArtistName = artist.name,
                ArtistId = artist.id
            };

            try
            {
                if (ShouldAutoApprove(RequestType.Album))
                {
                    model.Approved = true;
                    var hpSettings = HeadphonesService.GetSettings();

                    if (!hpSettings.Enabled)
                    {
                        await RequestService.AddRequestAsync(model);
                        return
                            Response.AsJson(new JsonResponseModel
                            {
                                Result = true,
                                Message = $"{model.Title} {Resources.UI.Search_SuccessfullyAdded}"
                            });
                    }

                    var sender = new HeadphonesSender(HeadphonesApi, hpSettings, RequestService);
                    await sender.AddAlbum(model);
                    return await AddRequest(model, settings, $"{model.Title} {Resources.UI.Search_SuccessfullyAdded}");
                }

                return await AddRequest(model, settings, $"{model.Title} {Resources.UI.Search_SuccessfullyAdded}");
            }
            catch (Exception e)
            {
                Log.Error(e);
                await FaultQueue.QueueItemAsync(model, albumInfo.id, RequestType.Album, FaultType.RequestFault, e.Message);

                await NotificationService.Publish(new NotificationModel
                {
                    DateTime = DateTime.Now,
                    User = Username,
                    RequestType = RequestType.Album,
                    Title = model.Title,
                    NotificationType = NotificationType.ItemAddedToFaultQueue
                });
                throw;
            }
        }

    }
}
