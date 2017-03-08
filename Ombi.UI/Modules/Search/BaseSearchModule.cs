#region Copyright

//  ************************************************************************
//    Copyright (c) 2016-2017 Ombi
//    File: BaseSearchModule.cs
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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses.Negotiation;
using Newtonsoft.Json;
using NLog;
using Ombi.Api;
using Ombi.Api.Interfaces;
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.Queue;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Helpers.Analytics;
using Ombi.Helpers.Permissions;
using Ombi.Services.Interfaces;
using Ombi.Services.Jobs;
using Ombi.Services.Notification;
using Ombi.Store;
using Ombi.Store.Models;
using Ombi.Store.Models.Emby;
using Ombi.Store.Models.Plex;
using Ombi.Store.Repository;
using Ombi.UI.Models;
using Action = Ombi.Helpers.Analytics.Action;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Modules
{
    public abstract class BaseSearchModule : BaseAuthModule
    {
        protected IRepository<PlexContent> PlexContentRepository { get; }
        protected IRepository<EmbyContent> EmbyContentRepository { get; }
        protected IPlexApi PlexApi { get; }
        protected INotificationService NotificationService { get; }
        protected IRequestService RequestService { get; }
        protected ICacheProvider Cache { get; }
        protected ISettingsService<AuthenticationSettings> Auth { get; }
        protected ISettingsService<EmbySettings> EmbySettings { get; }
        protected ISettingsService<PlexSettings> PlexService { get; }
        protected ISettingsService<PlexRequestSettings> PrService { get; }
        protected ISettingsService<EmailNotificationSettings> EmailNotificationSettings { get; }
        protected IAvailabilityChecker PlexChecker { get; }
        protected IEmbyAvailabilityChecker EmbyChecker { get; }
        protected IRepository<UsersToNotify> UsersToNotifyRepo { get; }
        protected IIssueService IssueService { get; }
        protected IAnalytics Analytics { get; }
        protected ITransientFaultQueue FaultQueue { get; }
        protected IRepository<RequestLimit> RequestLimitRepo { get; }
        protected ISettingsService<CustomizationSettings> CustomizationSettings { get; }

        protected static Logger Log = LogManager.GetCurrentClassLogger();

        public BaseSearchModule(IPlexApi plexApi, ISettingsService<PlexRequestSettings> prSettings,
            ISettingsService<PlexSettings> plexService, ISettingsService<AuthenticationSettings> auth,
            ISecurityExtensions security, IAvailabilityChecker plexChecker, INotificationService notify,
            ISettingsService<CustomizationSettings> cus, IRequestService request, IAnalytics a,
            IRepository<UsersToNotify> u, ISettingsService<EmailNotificationSettings> email,
            IIssueService issue, IRepository<RequestLimit> rl, ITransientFaultQueue tfQueue,
            IRepository<PlexContent> content, IEmbyAvailabilityChecker embyChecker,
            IRepository<EmbyContent> embyContent, ISettingsService<EmbySettings> embySettings)
            : base("search", prSettings, security)
        {
            Auth = auth;
            PlexService = plexService;
            PlexApi = plexApi;
            PrService = prSettings;
            PlexChecker = plexChecker;
            RequestService = request;
            NotificationService = notify;
            UsersToNotifyRepo = u;
            EmailNotificationSettings = email;
            IssueService = issue;
            Analytics = a;
            RequestLimitRepo = rl;
            FaultQueue = tfQueue;
            PlexContentRepository = content;
            CustomizationSettings = cus;
            EmbyChecker = embyChecker;
            EmbyContentRepository = embyContent;
            EmbySettings = embySettings;
        }

        protected bool CanUserSeeThisRequest(int movieId, bool usersCanViewOnlyOwnRequests,
            Dictionary<int, RequestedModel> moviesInDb)
        {
            if (usersCanViewOnlyOwnRequests)
            {
                var result = moviesInDb.FirstOrDefault(x => x.Value.ProviderId == movieId);
                return result.Value == null || result.Value.UserHasRequested(Username);
            }

            return true;
        }

        protected async Task<Response> AddUserToRequest(RequestedModel existingRequest, PlexRequestSettings settings,
            string fullShowName, bool episodeReq = false)
        {
            // check if the current user is already marked as a requester for this show, if not, add them
            if (!existingRequest.UserHasRequested(Username))
            {
                existingRequest.RequestedUsers.Add(Username);
            }
            if (Security.HasPermissions(User, Permissions.UsersCanViewOnlyOwnRequests) || episodeReq)
            {
                return
                    await
                        UpdateRequest(existingRequest, settings,
                            $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}");
            }

            return
                await UpdateRequest(existingRequest, settings, $"{fullShowName} {Resources.UI.Search_AlreadyRequested}");
        }

        protected bool ShouldSendNotification(RequestType type, PlexRequestSettings prSettings)
        {
            var sendNotification = ShouldAutoApprove(type)
                ? !prSettings.IgnoreNotifyForAutoApprovedRequests
                : true;

            if (IsAdmin)
            {
                sendNotification = false; // Don't bother sending a notification if the user is an admin

            }
            return sendNotification;
        }

        protected async Task<bool> CheckRequestLimit(PlexRequestSettings s, RequestType type)
        {
            if (IsAdmin)
                return true;

            if (Security.HasPermissions(User, Permissions.BypassRequestLimit))
                return true;

            var requestLimit = GetRequestLimitForType(type, s);
            if (requestLimit == 0)
            {
                return true;
            }

            var limit = await RequestLimitRepo.GetAllAsync();
            var usersLimit = limit.FirstOrDefault(x => x.Username == Username && x.RequestType == type);
            if (usersLimit == null)
            {
                // Have not set a requestLimit yet
                return true;
            }

            return requestLimit > usersLimit.RequestCount;
        }

        protected int GetRequestLimitForType(RequestType type, PlexRequestSettings s)
        {
            int requestLimit;
            switch (type)
            {
                case RequestType.Movie:
                    requestLimit = s.MovieWeeklyRequestLimit;
                    break;
                case RequestType.TvShow:
                    requestLimit = s.TvWeeklyRequestLimit;
                    break;
                case RequestType.Album:
                    requestLimit = s.AlbumWeeklyRequestLimit;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return requestLimit;
        }

        protected async Task<Response> AddRequest(RequestedModel model, PlexRequestSettings settings, string message)
        {
            await RequestService.AddRequestAsync(model);

            if (ShouldSendNotification(model.Type, settings))
            {
                var notificationModel = new NotificationModel
                {
                    Title = model.Title,
                    User = Username,
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.NewRequest,
                    RequestType = model.Type,
                    ImgSrc = model.Type == RequestType.Movie ? $"https://image.tmdb.org/t/p/w300/{model.PosterPath}" : model.PosterPath
                };
                await NotificationService.Publish(notificationModel);
            }

            var limit = await RequestLimitRepo.GetAllAsync();
            var usersLimit = limit.FirstOrDefault(x => x.Username == Username && x.RequestType == model.Type);
            if (usersLimit == null)
            {
                await RequestLimitRepo.InsertAsync(new RequestLimit
                {
                    Username = Username,
                    RequestType = model.Type,
                    FirstRequestDate = DateTime.UtcNow,
                    RequestCount = 1
                });
            }
            else
            {
                usersLimit.RequestCount++;
                await RequestLimitRepo.UpdateAsync(usersLimit);
            }

            return Response.AsJson(new JsonResponseModel { Result = true, Message = message });
        }

        protected async Task<Response> UpdateRequest(RequestedModel model, PlexRequestSettings settings, string message)
        {
            await RequestService.UpdateRequestAsync(model);

            if (ShouldSendNotification(model.Type, settings))
            {
                var notificationModel = new NotificationModel
                {
                    Title = model.Title,
                    User = Username,
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.NewRequest,
                    RequestType = model.Type,
                    ImgSrc = model.Type == RequestType.Movie ? $"https://image.tmdb.org/t/p/w300/{model.PosterPath}" : model.PosterPath
                };
                await NotificationService.Publish(notificationModel);
            }

            var limit = await RequestLimitRepo.GetAllAsync();
            var usersLimit = limit.FirstOrDefault(x => x.Username == Username && x.RequestType == model.Type);
            if (usersLimit == null)
            {
                await RequestLimitRepo.InsertAsync(new RequestLimit
                {
                    Username = Username,
                    RequestType = model.Type,
                    FirstRequestDate = DateTime.UtcNow,
                    RequestCount = 1
                });
            }
            else
            {
                usersLimit.RequestCount++;
                await RequestLimitRepo.UpdateAsync(usersLimit);
            }

            return Response.AsJson(new JsonResponseModel { Result = true, Message = message });
        }



        protected bool ShouldAutoApprove(RequestType requestType)
        {
            var admin = Security.HasPermissions(Context.CurrentUser, Permissions.Administrator);
            // if the user is an admin, they go ahead and allow auto-approval
            if (admin) return true;

            // check by request type if the category requires approval or not
            switch (requestType)
            {
                case RequestType.Movie:
                    return Security.HasPermissions(User, Permissions.AutoApproveMovie);
                case RequestType.TvShow:
                    return Security.HasPermissions(User, Permissions.AutoApproveTv);
                case RequestType.Album:
                    return Security.HasPermissions(User, Permissions.AutoApproveAlbum);
                default:
                    return false;
            }
        }


        protected string GetMediaServerName()
        {
            var e = EmbySettings.GetSettings();
            return e.Enable ? "Emby" : "Plex";
        }
    }
}

