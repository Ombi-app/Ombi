﻿using Ombi.Api.TheMovieDb;
using Ombi.Core.Models.Requests;
using Ombi.Helpers;
using Ombi.Store.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.UI;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Core.Models;

namespace Ombi.Core.Engine
{
    public class MovieRequestEngine : BaseMediaEngine, IMovieRequestEngine
    {
        public MovieRequestEngine(IMovieDbApi movieApi, IRequestServiceMain requestService, IPrincipal user,
            INotificationHelper helper, IRuleEvaluator r, IMovieSender sender, ILogger<MovieRequestEngine> log,
            OmbiUserManager manager, IRepository<RequestLog> rl, ICacheService cache,
            ISettingsService<OmbiSettings> ombiSettings, IRepository<RequestSubscription> sub)
            : base(user, requestService, r, manager, cache, ombiSettings, sub)
        {
            MovieApi = movieApi;
            NotificationHelper = helper;
            Sender = sender;
            Logger = log;
            _requestLog = rl;
        }

        private IMovieDbApi MovieApi { get; }
        private INotificationHelper NotificationHelper { get; }
        private IMovieSender Sender { get; }
        private ILogger<MovieRequestEngine> Logger { get; }
        private readonly IRepository<RequestLog> _requestLog;

        /// <summary>
        /// Requests the movie.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public async Task<RequestEngineResult> RequestMovie(MovieRequestViewModel model)
        {
            var movieInfo = await MovieApi.GetMovieInformationWithExtraInfo(model.TheMovieDbId, model.LanguageCode);
            if (movieInfo == null || movieInfo.Id == 0)
            {
                return new RequestEngineResult
                {
                    Result = false,
                    Message = "There was an issue adding this movie!",
                    ErrorMessage = $"Please try again later"
                };
            }

            var fullMovieName =
                $"{movieInfo.Title}{(!string.IsNullOrEmpty(movieInfo.ReleaseDate) ? $" ({DateTime.Parse(movieInfo.ReleaseDate).Year})" : string.Empty)}";

            var userDetails = await GetUser();

            var requestModel = new MovieRequests
            {
                TheMovieDbId = movieInfo.Id,
                RequestType = RequestType.Movie,
                Overview = movieInfo.Overview,
                ImdbId = movieInfo.ImdbId,
                PosterPath = PosterPathHelper.FixPosterPath(movieInfo.PosterPath),
                Title = movieInfo.Title,
                ReleaseDate = !string.IsNullOrEmpty(movieInfo.ReleaseDate)
                    ? DateTime.Parse(movieInfo.ReleaseDate)
                    : DateTime.MinValue,
                Status = movieInfo.Status,
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                RequestedUserId = userDetails.Id,
                Background = movieInfo.BackdropPath,
                LangCode = model.LanguageCode,
                RequestedByAlias = model.RequestedByAlias
            };

            var usDates = movieInfo.ReleaseDates?.Results?.FirstOrDefault(x => x.IsoCode == "US");
            requestModel.DigitalReleaseDate = usDates?.ReleaseDate
                ?.FirstOrDefault(x => x.Type == ReleaseDateType.Digital)?.ReleaseDate;

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
                var requestEngineResult = await AddMovieRequest(requestModel, fullMovieName);
                if (requestEngineResult.Result)
                {
                    var result = await ApproveMovie(requestModel);
                    if (result.IsError)
                    {
                        Logger.LogWarning("Tried auto sending movie but failed. Message: {0}", result.Message);
                        return new RequestEngineResult
                        {
                            Message = result.Message,
                            ErrorMessage = result.Message,
                            Result = false
                        };
                    }

                    return requestEngineResult;
                }

                // If there are no providers then it's successful but movie has not been sent
            }

            return await AddMovieRequest(requestModel, fullMovieName);
        }


        /// <summary>
        /// Gets the requests.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="position">The position.</param>
        /// <param name="orderFilter">The order/filter type.</param>
        /// <returns></returns>
        public async Task<RequestsViewModel<MovieRequests>> GetRequests(int count, int position,
            OrderFilterModel orderFilter)
        {
            var shouldHide = await HideFromOtherUsers();
            IQueryable<MovieRequests> allRequests;
            if (shouldHide.Hide)
            {
                allRequests =
                    MovieRepository.GetWithUser(shouldHide
                        .UserId); //.Skip(position).Take(count).OrderByDescending(x => x.ReleaseDate).ToListAsync();
            }
            else
            {
                allRequests =
                    MovieRepository
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

            var requests = await (OrderMovies(allRequests, orderFilter.OrderType)).Skip(position).Take(count)
                .ToListAsync();

            requests.ForEach(async x =>
            {
                x.PosterPath = PosterPathHelper.FixPosterPath(x.PosterPath);
                await CheckForSubscription(shouldHide, x);
            });
            return new RequestsViewModel<MovieRequests>
            {
                Collection = requests,
                Total = total
            };
        }

        public async Task<RequestsViewModel<MovieRequests>> GetRequests(int count, int position, string sortProperty, string sortOrder)
        {
            var shouldHide = await HideFromOtherUsers();
            IQueryable<MovieRequests> allRequests;
            if (shouldHide.Hide)
            {
                allRequests =
                    MovieRepository.GetWithUser(shouldHide
                        .UserId); 
            }
            else
            {
                allRequests =
                    MovieRepository
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

            allRequests = sortOrder.Equals("asc", StringComparison.InvariantCultureIgnoreCase) 
                ? allRequests.OrderBy(x => prop.GetValue(x)) 
                : allRequests.OrderByDescending(x => prop.GetValue(x));
            var total = await allRequests.CountAsync();
            var requests = await allRequests.Skip(position).Take(count)
                .ToListAsync();
            requests.ForEach(async x =>
            {
                x.PosterPath = PosterPathHelper.FixPosterPath(x.PosterPath);
                await CheckForSubscription(shouldHide, x);
            });
            return new RequestsViewModel<MovieRequests>
            {
                Collection = requests,
                Total = total
            };

        }

        private IQueryable<MovieRequests> OrderMovies(IQueryable<MovieRequests> allRequests, OrderType type)
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
                case OrderType.StatusAsc:
                    return allRequests.OrderBy(x => x.Status);
                case OrderType.StatusDesc:
                    return allRequests.OrderByDescending(x => x.Status);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public async Task<int> GetTotal()
        {
            var shouldHide = await HideFromOtherUsers();
            if (shouldHide.Hide)
            {
                return await MovieRepository.GetWithUser(shouldHide.UserId).CountAsync();
            }
            else
            {
                return await MovieRepository.GetWithUser().CountAsync();
            }
        }

        /// <summary>
        /// Gets the requests.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<MovieRequests>> GetRequests()
        {
            var shouldHide = await HideFromOtherUsers();
            List<MovieRequests> allRequests;
            if (shouldHide.Hide)
            {
                allRequests = await MovieRepository.GetWithUser(shouldHide.UserId).ToListAsync();
            }
            else
            {
                allRequests = await MovieRepository.GetWithUser().ToListAsync();
            }

            allRequests.ForEach(async x =>
            {
                x.PosterPath = PosterPathHelper.FixPosterPath(x.PosterPath);
                await CheckForSubscription(shouldHide, x);
            });
            return allRequests;
        }

        public async Task<MovieRequests> GetRequest(int requestId)
        {
            var request = await MovieRepository.GetWithUser().Where(x => x.Id == requestId).FirstOrDefaultAsync();
            request.PosterPath = PosterPathHelper.FixPosterPath(request.PosterPath);
            await CheckForSubscription(new HideResult(), request);

            return request;
        }

        private async Task CheckForSubscription(HideResult shouldHide, MovieRequests x)
        {
            if (shouldHide.UserId == x.RequestedUserId)
            {
                x.ShowSubscribe = false;
            }
            else
            {
                x.ShowSubscribe = true;
                var sub = await _subscriptionRepository.GetAll().FirstOrDefaultAsync(s =>
                    s.UserId == shouldHide.UserId && s.RequestId == x.Id && s.RequestType == RequestType.Movie);
                x.Subscribed = sub != null;
            }
        }

        /// <summary>
        /// Searches the movie request.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <returns></returns>
        public async Task<IEnumerable<MovieRequests>> SearchMovieRequest(string search)
        {
            var shouldHide = await HideFromOtherUsers();
            List<MovieRequests> allRequests;
            if (shouldHide.Hide)
            {
                allRequests = await MovieRepository.GetWithUser(shouldHide.UserId).ToListAsync();
            }
            else
            {
                allRequests = await MovieRepository.GetWithUser().ToListAsync();
            }

            var results = allRequests.Where(x => x.Title.Contains(search, CompareOptions.IgnoreCase)).ToList();
            results.ForEach(async x =>
            {
                x.PosterPath = PosterPathHelper.FixPosterPath(x.PosterPath);
                await CheckForSubscription(shouldHide, x);
            });
            return results;
        }

        public async Task<RequestEngineResult> ApproveMovieById(int requestId)
        {
            var request = await MovieRepository.Find(requestId);
            return await ApproveMovie(request);
        }

        public async Task<RequestEngineResult> DenyMovieById(int modelId, string denyReason)
        {
            var request = await MovieRepository.Find(modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Request does not exist"
                };
            }

            request.Denied = true;
            request.DeniedReason = denyReason;
            // We are denying a request
            NotificationHelper.Notify(request, NotificationType.RequestDeclined);
            await MovieRepository.Update(request);

            return new RequestEngineResult
            {
                Result = true,
                Message = "Request successfully deleted",
            };
        }

        public async Task<RequestEngineResult> ApproveMovie(MovieRequests request)
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
            await MovieRepository.Update(request);

            var canNotify = await RunSpecificRule(request, SpecificRules.CanSendNotification);
            if (canNotify.Success)
            {
                NotificationHelper.Notify(request, NotificationType.RequestApproved);
            }

            if (request.Approved)
            {
                var result = await Sender.Send(request);
                if (result.Success && result.Sent)
                {
                    return new RequestEngineResult
                    {
                        Result = true
                    };
                }

                if (!result.Success)
                {
                    Logger.LogWarning("Tried auto sending movie but failed. Message: {0}", result.Message);
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
        /// Updates the movie request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<MovieRequests> UpdateMovieRequest(MovieRequests request)
        {
            var allRequests = await MovieRepository.GetWithUser().ToListAsync();
            var results = allRequests.FirstOrDefault(x => x.Id == request.Id);

            results.Approved = request.Approved;
            results.Available = request.Available;
            results.Denied = request.Denied;
            results.DeniedReason = request.DeniedReason;
            results.ImdbId = request.ImdbId;
            results.IssueId = request.IssueId;
            results.Issues = request.Issues;
            results.Overview = request.Overview;
            results.PosterPath = PosterPathHelper.FixPosterPath(request.PosterPath);
            results.QualityOverride = request.QualityOverride;
            results.RootPathOverride = request.RootPathOverride;

            await MovieRepository.Update(results);
            return results;
        }

        /// <summary>
        /// Removes the movie request.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns></returns>
        public async Task RemoveMovieRequest(int requestId)
        {
            var request = await MovieRepository.GetAll().FirstOrDefaultAsync(x => x.Id == requestId);
            await MovieRepository.Delete(request);
        }

        public async Task RemoveAllMovieRequests()
        {
            var request = MovieRepository.GetAll();
            await MovieRepository.DeleteRange(request);
        }

        public async Task<bool> UserHasRequest(string userId)
        {
            return await MovieRepository.GetAll().AnyAsync(x => x.RequestedUserId == userId);
        }

        public async Task<RequestEngineResult> MarkUnavailable(int modelId)
        {
            var request = await MovieRepository.Find(modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Request does not exist"
                };
            }

            request.Available = false;
            await MovieRepository.Update(request);

            return new RequestEngineResult
            {
                Message = "Request is now unavailable",
                Result = true
            };
        }

        public async Task<RequestEngineResult> MarkAvailable(int modelId)
        {
            var request = await MovieRepository.Find(modelId);
            if (request == null)
            {
                return new RequestEngineResult
                {
                    ErrorMessage = "Request does not exist"
                };
            }

            request.Available = true;
            request.MarkedAsAvailable = DateTime.Now;
            NotificationHelper.Notify(request, NotificationType.RequestAvailable);
            await MovieRepository.Update(request);

            return new RequestEngineResult
            {
                Message = "Request is now available",
                Result = true
            };
        }

        private async Task<RequestEngineResult> AddMovieRequest(MovieRequests model, string movieName)
        {
            await MovieRepository.Add(model);

            var result = await RunSpecificRule(model, SpecificRules.CanSendNotification);
            if (result.Success)
            {
                NotificationHelper.NewRequest(model);
            }

            await _requestLog.Add(new RequestLog
            {
                UserId = (await GetUser()).Id,
                RequestDate = DateTime.UtcNow,
                RequestId = model.Id,
                RequestType = RequestType.Movie,
            });

            return new RequestEngineResult { Result = true, Message = $"{movieName} has been successfully added!", RequestId = model.Id };
        }

        public async Task<RequestQuotaCountModel> GetRemainingRequests(OmbiUser user)
        {
            if (user == null)
            {
                user = await GetUser();

                // If user is still null after attempting to get the logged in user, return null.
                if (user == null)
                {
                    return null;
                }
            }

            int limit = user.MovieRequestLimit ?? 0;

            if (limit <= 0)
            {
                return new RequestQuotaCountModel()
                {
                    HasLimit = false,
                    Limit = 0,
                    Remaining = 0,
                    NextRequest = DateTime.Now,
                };
            }

            IQueryable<RequestLog> log = _requestLog.GetAll().Where(x => x.UserId == user.Id && x.RequestType == RequestType.Movie);

            int count = limit - await log.CountAsync(x => x.RequestDate >= DateTime.UtcNow.AddDays(-7));

            DateTime oldestRequestedAt = await log.Where(x => x.RequestDate >= DateTime.UtcNow.AddDays(-7))
                                            .OrderBy(x => x.RequestDate)
                                            .Select(x => x.RequestDate)
                                            .FirstOrDefaultAsync();

            return new RequestQuotaCountModel()
            {
                HasLimit = true,
                Limit = limit,
                Remaining = count,
                NextRequest = DateTime.SpecifyKind(oldestRequestedAt.AddDays(7), DateTimeKind.Utc),
            };
        }
    }
}