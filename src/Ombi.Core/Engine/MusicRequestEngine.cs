﻿using Ombi.Api.TheMovieDb;
using Ombi.Core.Models.Requests;
using Ombi.Helpers;
using Ombi.Store.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Lidarr;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models;
using Ombi.Core.Models.UI;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Senders;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System.ComponentModel;

namespace Ombi.Core.Engine
{
    public class MusicRequestEngine : BaseMediaEngine, IMusicRequestEngine
    {
        public MusicRequestEngine(IRequestServiceMain requestService, IPrincipal user,
            INotificationHelper helper, IRuleEvaluator r, ILogger<MusicRequestEngine> log,
            OmbiUserManager manager, IRepository<RequestLog> rl, ICacheService cache,
            ISettingsService<OmbiSettings> ombiSettings, IRepository<RequestSubscription> sub, ILidarrApi lidarr,
            ISettingsService<LidarrSettings> lidarrSettings, IMusicSender sender)
            : base(user, requestService, r, manager, cache, ombiSettings, sub)
        {
            NotificationHelper = helper;
            _musicSender = sender;
            Logger = log;
            _requestLog = rl;
            _lidarrApi = lidarr;
            _lidarrSettings = lidarrSettings;
        }

        private INotificationHelper NotificationHelper { get; }
        //private IMovieSender Sender { get; }
        private ILogger Logger { get; }
        private readonly IRepository<RequestLog> _requestLog;
        private readonly ISettingsService<LidarrSettings> _lidarrSettings;
        private readonly ILidarrApi _lidarrApi;
        private readonly IMusicSender _musicSender;

        /// <summary>
        /// Requests the Album.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public async Task<RequestEngineResult> RequestAlbum(MusicAlbumRequestViewModel model)
        {
            var s = await _lidarrSettings.GetSettingsAsync();
            var album = await _lidarrApi.GetAlbumByForeignId(model.ForeignAlbumId, s.ApiKey, s.FullUri);
            if (album == null)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    Message = "There was an issue adding this album!",
                    ErrorMessage = "Please try again later"
                };
            }

            var userDetails = await GetUser();

            var requestModel = new AlbumRequest
            {
                ForeignAlbumId = model.ForeignAlbumId,
                ArtistName = album.artist?.artistName,
                ReleaseDate = album.releaseDate,
                RequestedDate = DateTime.Now,
                RequestType = RequestType.Album,
                Rating = album.ratings?.value ?? 0m,
                RequestedUserId = userDetails.Id,
                Title = album.title,
                Disk = album.images?.FirstOrDefault(x => x.coverType.Equals("disc"))?.url,
                Cover = album.images?.FirstOrDefault(x => x.coverType.Equals("cover"))?.url,
                ForeignArtistId = album?.artist?.foreignArtistId ?? string.Empty, // This needs to be populated to send to Lidarr for new requests
                RequestedByAlias = model.RequestedByAlias
            };
            if (requestModel.Cover.IsNullOrEmpty())
            {
                requestModel.Cover = album.remoteCover;
            }

            var ruleResults = (await RunRequestRules(requestModel)).ToList();
            if (ruleResults.Any(x => !x.Success))
            {
                return new RequestEngineResult
                {
                    ErrorMessage = ruleResults.FirstOrDefault(x => x.Message.HasValue()).Message
                };
            }

            if (requestModel.Approved) // The rules have auto approved this
            {
                var requestEngineResult = await AddAlbumRequest(requestModel);
                if (requestEngineResult.Result)
                {
                    var result = await ApproveAlbum(requestModel);
                    if (result.IsError)
                    {
                        Logger.LogWarning("Tried auto sending Album but failed. Message: {0}", result.Message);
                        return new RequestEngineResult
                        {
                            Message = result.Message,
                            ErrorMessage = result.Message,
                            Result = false
                        };
                    }

                    return requestEngineResult;
                }

                // If there are no providers then it's successful but album has not been sent
            }

            return await AddAlbumRequest(requestModel);
        }

        /// <summary>
        /// Gets the requests.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="position">The position.</param>
        /// <param name="orderFilter">The order/filter type.</param>
        /// <returns></returns>
        public async Task<RequestsViewModel<AlbumRequest>> GetRequests(int count, int position,
            OrderFilterModel orderFilter)
        {
            var shouldHide = await HideFromOtherUsers();
            IQueryable<AlbumRequest> allRequests;
            if (shouldHide.Hide)
            {
                allRequests =
                    MusicRepository.GetWithUser(shouldHide
                        .UserId); //.Skip(position).Take(count).OrderByDescending(x => x.ReleaseDate).ToListAsync();
            }
            else
            {
                allRequests =
                    MusicRepository
                        .GetWithUser(); //.Skip(position).Take(count).OrderByDescending(x => x.ReleaseDate).ToListAsync();
            }

            switch (orderFilter.AvailabilityFilter)
            {
                case FilterType.None:
                    break;
                case FilterType.Available:
                    allRequests = allRequests.Where(x => x.Available);
                    break;
                case FilterType.NotAvailable:
                    allRequests = allRequests.Where(x => !x.Available);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (orderFilter.StatusFilter)
            {
                case FilterType.None:
                    break;
                case FilterType.Approved:
                    allRequests = allRequests.Where(x => x.Approved);
                    break;
                case FilterType.Processing:
                    allRequests = allRequests.Where(x => x.Approved && !x.Available);
                    break;
                case FilterType.PendingApproval:
                    allRequests = allRequests.Where(x => !x.Approved && !x.Available && !(x.Denied ?? false));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var total = allRequests.Count();

            var requests = await (OrderAlbums(allRequests, orderFilter.OrderType)).Skip(position).Take(count)
                .ToListAsync();

            requests.ForEach(async x =>
            {
                await CheckForSubscription(shouldHide, x);
            });
            return new RequestsViewModel<AlbumRequest>
            {
                Collection = requests,
                Total = total
            };
        }

        private IQueryable<AlbumRequest> OrderAlbums(IQueryable<AlbumRequest> allRequests, OrderType type)
        {
            switch (type)
            {
                case OrderType.RequestedDateAsc:
                    return allRequests.OrderBy(x => x.RequestedDate);
                case OrderType.RequestedDateDesc:
                    return allRequests.OrderByDescending(x => x.RequestedDate);
                case OrderType.TitleAsc:
                    return allRequests.OrderBy(x => x.Title);
                case OrderType.TitleDesc:
                    return allRequests.OrderByDescending(x => x.Title);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public async Task<int> GetTotal()
        {
            var shouldHide = await HideFromOtherUsers();
            if (shouldHide.Hide)
            {
                return await MusicRepository.GetWithUser(shouldHide.UserId).CountAsync();
            }
            else
            {
                return await MusicRepository.GetWithUser().CountAsync();
            }
        }

        /// <summary>
        /// Gets the requests.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<AlbumRequest>> GetRequests()
        {
            var shouldHide = await HideFromOtherUsers();
            List<AlbumRequest> allRequests;
            if (shouldHide.Hide)
            {
                allRequests = await MusicRepository.GetWithUser(shouldHide.UserId).ToListAsync();
            }
            else
            {
                allRequests = await MusicRepository.GetWithUser().ToListAsync();
            }

            allRequests.ForEach(async x =>
            {
                await CheckForSubscription(shouldHide, x);
            });
            return allRequests;
        }

        
        private async Task CheckForSubscription(HideResult shouldHide, List<AlbumRequest> albumRequests)
        {
            var requestIds = albumRequests.Select(x => x.Id);
            var sub = await _subscriptionRepository.GetAll().Where(s =>
                s.UserId == shouldHide.UserId && requestIds.Contains(s.RequestId) && s.RequestType == RequestType.Movie)
                .ToListAsync();
            foreach (var x in albumRequests)
            {
                if (shouldHide.UserId == x.RequestedUserId)
                {
                    x.ShowSubscribe = false;
                }
                else
                {
                    x.ShowSubscribe = true;
                    var hasSub = sub.FirstOrDefault(r => r.RequestId == x.Id);
                    x.Subscribed = hasSub != null;
                }
            }
        }

        private async Task CheckForSubscription(HideResult shouldHide, AlbumRequest x)
        {
            if (shouldHide.UserId == x.RequestedUserId)
            {
                x.ShowSubscribe = false;
            }
            else
            {
                x.ShowSubscribe = true;
                var sub = await _subscriptionRepository.GetAll().FirstOrDefaultAsync(s =>
                    s.UserId == shouldHide.UserId && s.RequestId == x.Id && s.RequestType == RequestType.Album);
                x.Subscribed = sub != null;
            }
        }

        /// <summary>
        /// Searches the album request.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <returns></returns>
        public async Task<IEnumerable<AlbumRequest>> SearchAlbumRequest(string search)
        {
            var shouldHide = await HideFromOtherUsers();
            List<AlbumRequest> allRequests;
            if (shouldHide.Hide)
            {
                allRequests = await MusicRepository.GetWithUser(shouldHide.UserId).ToListAsync();
            }
            else
            {
                allRequests = await MusicRepository.GetWithUser().ToListAsync();
            }

            var results = allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase)).ToList();
            results.ForEach(async x =>
            {
                await CheckForSubscription(shouldHide, x);
            });
            return results;
        }

        public async Task<RequestEngineResult> ApproveAlbumById(int requestId)
        {
            var request = await MusicRepository.Find(requestId);
            return await ApproveAlbum(request);
        }

        public async Task<RequestEngineResult> DenyAlbumById(int modelId, string reason)
        {
            var request = await MusicRepository.Find(modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Request does not exist"
                };
            }

            request.Denied = true;
            request.DeniedReason = reason;
            // We are denying a request
            await NotificationHelper.Notify(request, NotificationType.RequestDeclined);
            await MusicRepository.Update(request);

            return new RequestEngineResult
            {
                Message = "Request successfully deleted",
            };
        }

        public async Task<RequestEngineResult> ApproveAlbum(AlbumRequest request)
        {
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Request does not exist"
                };
            }

            request.MarkedAsApproved = DateTime.Now;
            request.Approved = true;
            request.Denied = false;
            await MusicRepository.Update(request);


            var canNotify = await RunSpecificRule(request, SpecificRules.CanSendNotification, string.Empty);
            if (canNotify.Success)
            {
                await NotificationHelper.Notify(request, NotificationType.RequestApproved);
            }

            if (request.Approved)
            {
                var result = await _musicSender.Send(request);
                if (result.Success && result.Sent)
                {
                    return new RequestEngineResult
                    {
                        Result = true
                    };
                }

                if (!result.Success)
                {
                    Logger.LogWarning("Tried auto sending album but failed. Message: {0}", result.Message);
                    return new RequestEngineResult
                    {
                        Message = result.Message,
                        ErrorMessage = result.Message,
                        Result = false
                    };
                }

                // If there are no providers then it's successful but movie has not been sent
            }

            return new RequestEngineResult
            {
                Result = true
            };
        }

        /// <summary>
        /// Removes the Album request.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns></returns>
        public async Task<RequestEngineResult> RemoveAlbumRequest(int requestId)
        {
            var request = await MusicRepository.GetAll().FirstOrDefaultAsync(x => x.Id == requestId);
            
            var result = await CheckCanManageRequest(request);
            if (result.IsError)
                return result;

            await MusicRepository.Delete(request);

            return new RequestEngineResult
            {
                Result = true,
            };
        }

        public async Task<bool> UserHasRequest(string userId)
        {
            return await MusicRepository.GetAll().AnyAsync(x => x.RequestedUserId == userId);
        }

        public async Task<RequestEngineResult> MarkUnavailable(int modelId)
        {
            var request = await MusicRepository.Find(modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Request does not exist"
                };
            }

            request.Available = false;
            await MusicRepository.Update(request);

            return new RequestEngineResult
            {
                Message = "Request is now unavailable",
                Result = true
            };
        }

        public async Task<RequestEngineResult> MarkAvailable(int modelId)
        {
            var request = await MusicRepository.Find(modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Request does not exist"
                };
            }

            request.Available = true;
            request.MarkedAsAvailable = DateTime.Now;
            await NotificationHelper.Notify(request, NotificationType.RequestAvailable);
            await MusicRepository.Update(request);

            return new RequestEngineResult
            {
                Message = "Request is now available",
                Result = true
            };
        }

        private async Task<RequestEngineResult> AddAlbumRequest(AlbumRequest model)
        {
            await MusicRepository.Add(model);

            var result = await RunSpecificRule(model, SpecificRules.CanSendNotification, string.Empty);
            if (result.Success)
            {
                await NotificationHelper.NewRequest(model);
            }

            await _requestLog.Add(new RequestLog
            {
                UserId = (await GetUser()).Id,
                RequestDate = DateTime.UtcNow,
                RequestId = model.Id,
                RequestType = RequestType.Album,
            });

            return new RequestEngineResult { Result = true, Message = $"{model.Title} has been successfully added!", RequestId = model.Id };
        }

        public async Task<RequestsViewModel<AlbumRequest>> GetRequestsByStatus(int count, int position, string sortProperty, string sortOrder, RequestStatus status)
        {
             var shouldHide = await HideFromOtherUsers();
            IQueryable<AlbumRequest> allRequests;
            if (shouldHide.Hide)
            {
                allRequests =
                    MusicRepository.GetWithUser(shouldHide
                        .UserId);
            }
            else
            {
                allRequests =
                    MusicRepository
                        .GetWithUser();
            }

            switch (status)
            {
                case RequestStatus.PendingApproval:
                    allRequests = allRequests.Where(x => !x.Approved && !x.Available && (!x.Denied.HasValue || !x.Denied.Value));
                    break;
                case RequestStatus.ProcessingRequest:
                    allRequests = allRequests.Where(x => x.Approved && !x.Available && (!x.Denied.HasValue || !x.Denied.Value));
                    break;
                case RequestStatus.Available:
                    allRequests = allRequests.Where(x => x.Available);
                    break;
                case RequestStatus.Denied:
                    allRequests = allRequests.Where(x => x.Denied.HasValue  && x.Denied.Value && !x.Available);
                    break;
                default:
                    break;
            }

            var prop = TypeDescriptor.GetProperties(typeof(AlbumRequest)).Find(sortProperty, true);

            if (sortProperty.Contains('.'))
            {
                // This is a navigation property currently not supported
                prop = TypeDescriptor.GetProperties(typeof(AlbumRequest)).Find("RequestedDate", true);
                //var properties = sortProperty.Split(new []{'.'}, StringSplitOptions.RemoveEmptyEntries);
                //var firstProp = TypeDescriptor.GetProperties(typeof(MovieRequests)).Find(properties[0], true);
                //var propType = firstProp.PropertyType;
                //var secondProp = TypeDescriptor.GetProperties(propType).Find(properties[1], true);
            }

            // TODO fix this so we execute this on the server
            var requests = sortOrder.Equals("asc", StringComparison.InvariantCultureIgnoreCase)
                ? allRequests.ToList().OrderBy(x => x.RequestedDate).ToList()
                : allRequests.ToList().OrderByDescending(x => prop.GetValue(x)).ToList();
            var total = requests.Count();
            requests = requests.Skip(position).Take(count).ToList();

            await CheckForSubscription(shouldHide, requests);
            return new RequestsViewModel<AlbumRequest>
            {
                Collection = requests,
                Total = total
            };
        }

        public async Task<RequestsViewModel<AlbumRequest>> GetRequests(int count, int position, string sortProperty, string sortOrder)
        { 
            var shouldHide = await HideFromOtherUsers();
            IQueryable<AlbumRequest> allRequests;
            if (shouldHide.Hide)
            {
                allRequests =
                    MusicRepository.GetWithUser(shouldHide
                        .UserId);
            }
            else
            {
                allRequests =
                    MusicRepository
                        .GetWithUser();
            }

            var prop = TypeDescriptor.GetProperties(typeof(MovieRequests)).Find(sortProperty, true);

            if (sortProperty.Contains('.'))
            {
                // This is a navigation property currently not supported
                prop = TypeDescriptor.GetProperties(typeof(MovieRequests)).Find("RequestedDate", true);
                //var properties = sortProperty.Split(new []{'.'}, StringSplitOptions.RemoveEmptyEntries);
                //var firstProp = TypeDescriptor.GetProperties(typeof(MovieRequests)).Find(properties[0], true);
                //var propType = firstProp.PropertyType;
                //var secondProp = TypeDescriptor.GetProperties(propType).Find(properties[1], true);
            }

            // TODO fix this so we execute this on the server
            var requests = sortOrder.Equals("asc", StringComparison.InvariantCultureIgnoreCase)
                ? allRequests.ToList().OrderBy(x => x.RequestedDate).ToList()
                : allRequests.ToList().OrderByDescending(x => prop.GetValue(x)).ToList();
            var total = requests.Count();
            requests = requests.Skip(position).Take(count).ToList();

            await CheckForSubscription(shouldHide, requests);
            return new RequestsViewModel<AlbumRequest>
            {
                Collection = requests,
                Total = total
            };
        }
    }
}