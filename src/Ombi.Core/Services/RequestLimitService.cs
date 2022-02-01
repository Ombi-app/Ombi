using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Models;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Ombi.Core.Services
{
    public interface IRequestLimitService
    {
        Task<RequestQuotaCountModel> GetRemainingMovieRequests(OmbiUser user = default, DateTime now = default);
        Task<RequestQuotaCountModel> GetRemainingTvRequests(OmbiUser user = default, DateTime now = default);
        Task<RequestQuotaCountModel> GetRemainingMusicRequests(OmbiUser user = default, DateTime now = default);
    }
    public class RequestLimitService : IRequestLimitService
    {
        private readonly IPrincipal _user;
        private readonly OmbiUserManager _userManager;
        private readonly IRepository<RequestLog> _requestLog;

        public RequestLimitService(IPrincipal user, OmbiUserManager userManager, IRepository<RequestLog> rl)
        {
            _user = user;
            _userManager = userManager;
            _requestLog = rl;
        }

        public async Task<RequestQuotaCountModel> GetRemainingMovieRequests(OmbiUser user, DateTime now = default)
        {
            if (now == default)
            {
                now = DateTime.UtcNow;
            }
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

            if (!user.MovieRequestLimitType.HasValue)
            {
                var count = limit - await log.CountAsync(x => x.RequestDate >= DateTime.UtcNow.AddDays(-7));

                var oldestRequestedAt = await log.Where(x => x.RequestDate >= DateTime.UtcNow.AddDays(-7))
                                                .OrderBy(x => x.RequestDate)
                                                .Select(x => x.RequestDate)
                                                .FirstOrDefaultAsync();

                return new RequestQuotaCountModel()
                {
                    HasLimit = true,
                    Limit = limit,
                    Remaining = count < 0 ? 0 : count,
                    NextRequest = DateTime.SpecifyKind(oldestRequestedAt.AddDays(7), DateTimeKind.Utc),
                };
            }


            return await CalculateBasicRemaingRequests(limit, user.MovieRequestLimitType ?? RequestLimitType.Day, log, now);
        }

        public async Task<RequestQuotaCountModel> GetRemainingMusicRequests(OmbiUser user, DateTime now = default)
        {
            if (now == default)
            {
                now = DateTime.UtcNow;
            }
            if (user == null)
            {
                user = await GetUser();

                // If user is still null after attempting to get the logged in user, return null.
                if (user == null)
                {
                    return null;
                }
            }

            int limit = user.MusicRequestLimit ?? 0;

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

            IQueryable<RequestLog> log = _requestLog.GetAll().Where(x => x.UserId == user.Id && x.RequestType == RequestType.Album);

            // Hisoric Limits
            if (!user.MusicRequestLimitType.HasValue)
            {
                var oldcount = limit - await log.CountAsync(x => x.RequestDate >= DateTime.UtcNow.AddDays(-7));

                var oldestRequestedAtOld = await log.Where(x => x.RequestDate >= DateTime.UtcNow.AddDays(-7))
                    .OrderBy(x => x.RequestDate)
                    .Select(x => x.RequestDate)
                    .FirstOrDefaultAsync();

                return new RequestQuotaCountModel()
                {
                    HasLimit = true,
                    Limit = limit,
                    Remaining = oldcount < 0 ? 0 : oldcount,
                    NextRequest = DateTime.SpecifyKind(oldestRequestedAtOld.AddDays(7), DateTimeKind.Utc),
                };
            }

            return await CalculateBasicRemaingRequests(limit, user.MusicRequestLimitType ?? RequestLimitType.Day, log, now);
        }

        private async Task<OmbiUser> GetUser()
        {
            var username = _user.Identity.Name.ToUpper();
            return await _userManager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username);
        }

        private static async Task<RequestQuotaCountModel> CalculateBasicRemaingRequests(int limit, RequestLimitType type, IQueryable<RequestLog> log, DateTime now)
        {
            int count = 0;
            DateTime oldestRequestedAt = DateTime.Now;
            DateTime nextRequest = DateTime.Now;
            switch (type)
            {
                case RequestLimitType.Day:
                    count = limit - await log.CountAsync(x => x.RequestDate >= now.Date);
                    oldestRequestedAt = await log.Where(x => x.RequestDate >= now.Date)
                                            .OrderBy(x => x.RequestDate)
                                            .Select(x => x.RequestDate)
                                            .FirstOrDefaultAsync();
                    nextRequest = oldestRequestedAt.AddDays(1).Date;
                    break;
                case RequestLimitType.Week:
                    var fdow = now.FirstDateInWeek().Date;
                    count = limit - await log.CountAsync(x => x.RequestDate >= fdow);
                    oldestRequestedAt = await log.Where(x => x.RequestDate >= fdow)
                                            .OrderBy(x => x.RequestDate)
                                            .Select(x => x.RequestDate)
                                            .FirstOrDefaultAsync();
                    nextRequest = fdow.AddDays(7).Date;
                    break;
                case RequestLimitType.Month:
                    var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                    count = limit - await log.CountAsync(x => x.RequestDate >= firstDayOfMonth);
                    oldestRequestedAt = await log.Where(x => x.RequestDate >= firstDayOfMonth)
                                            .OrderBy(x => x.RequestDate)
                                            .Select(x => x.RequestDate)
                                            .FirstOrDefaultAsync();
                    nextRequest = firstDayOfMonth.AddMonths(1).Date;
                    break;
            }

            return new RequestQuotaCountModel()
            {
                HasLimit = true,
                Limit = limit,
                Remaining = count < 0 ? 0 : count,
                NextRequest = DateTime.SpecifyKind(nextRequest, DateTimeKind.Utc),
            };
        }

        public async Task<RequestQuotaCountModel> GetRemainingTvRequests(OmbiUser user, DateTime now = default)
        {
            if (now == default)
            {
                now = DateTime.UtcNow;
            }
            if (user == null)
            {
                user = await GetUser();

                // If user is still null after attempting to get the logged in user, return null.
                if (user == null)
                {
                    return null;
                }
            }

            int limit = user.EpisodeRequestLimit ?? 0;

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

            IQueryable<RequestLog> log = _requestLog.GetAll().Where(x => x.UserId == user.Id && x.RequestType == RequestType.TvShow);

            int count = 0;
            DateTime oldestRequestedAt = DateTime.Now;
            DateTime nextRequest = DateTime.Now;


            IQueryable<RequestLog> filteredLog;
            int zeroEpisodeCount;
            int episodeCount;

            if (!user.EpisodeRequestLimitType.HasValue)
            {
                filteredLog = log.Where(x => x.RequestDate >= DateTime.UtcNow.AddDays(-7));
                // Needed, due to a bug which would cause all episode counts to be 0
                zeroEpisodeCount = await filteredLog.Where(x => x.EpisodeCount == 0).Select(x => x.EpisodeCount).CountAsync();

                episodeCount = await filteredLog.Where(x => x.EpisodeCount != 0).Select(x => x.EpisodeCount).SumAsync();

                count = limit - (zeroEpisodeCount + episodeCount);

                oldestRequestedAt = await log
                                                .Where(x => x.RequestDate >= now.AddDays(-7))
                                                .OrderBy(x => x.RequestDate)
                                                .Select(x => x.RequestDate)
                                                .FirstOrDefaultAsync();

                return new RequestQuotaCountModel()
                {
                    HasLimit = true,
                    Limit = limit,
                    Remaining = count < 0 ? 0 : count,
                    NextRequest = DateTime.SpecifyKind(oldestRequestedAt.AddDays(7), DateTimeKind.Utc).Date,
                };
            }

            switch (user.EpisodeRequestLimitType)
            {
                case RequestLimitType.Day:

                    filteredLog = log.Where(x => x.RequestDate >= DateTime.UtcNow.Date);
                    // Needed, due to a bug which would cause all episode counts to be 0
                    zeroEpisodeCount = await filteredLog.Where(x => x.EpisodeCount == 0).Select(x => x.EpisodeCount).CountAsync();
                    episodeCount = await filteredLog.Where(x => x.EpisodeCount != 0).Select(x => x.EpisodeCount).SumAsync();
                    count = limit - (zeroEpisodeCount + episodeCount);

                    oldestRequestedAt = await log.Where(x => x.RequestDate >= now.Date)
                                            .OrderBy(x => x.RequestDate)
                                            .Select(x => x.RequestDate)
                                            .FirstOrDefaultAsync();
                    nextRequest = oldestRequestedAt.AddDays(1).Date;
                    break;
                case RequestLimitType.Week:
                    var fdow = now.FirstDateInWeek().Date;
                    filteredLog = log.Where(x => x.RequestDate >= now.Date.AddDays(-7));
                    // Needed, due to a bug which would cause all episode counts to be 0
                    zeroEpisodeCount = await filteredLog.Where(x => x.EpisodeCount == 0).Select(x => x.EpisodeCount).CountAsync();
                    episodeCount = await filteredLog.Where(x => x.EpisodeCount != 0).Select(x => x.EpisodeCount).SumAsync();
                    count = limit - (zeroEpisodeCount + episodeCount);

                    oldestRequestedAt = await log.Where(x => x.RequestDate >= now.Date.AddDays(-7))
                                            .OrderBy(x => x.RequestDate)
                                            .Select(x => x.RequestDate)
                                            .FirstOrDefaultAsync();
                    nextRequest = fdow.AddDays(7).Date;
                    break;
                case RequestLimitType.Month:
                    var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                    filteredLog = log.Where(x => x.RequestDate >= now.Date.AddMonths(-1));
                    // Needed, due to a bug which would cause all episode counts to be 0
                    zeroEpisodeCount = await filteredLog.Where(x => x.EpisodeCount == 0).Select(x => x.EpisodeCount).CountAsync();
                    episodeCount = await filteredLog.Where(x => x.EpisodeCount != 0).Select(x => x.EpisodeCount).SumAsync();
                    count = limit - (zeroEpisodeCount + episodeCount);

                    oldestRequestedAt = await log.Where(x => x.RequestDate >= now.Date.AddMonths(-1))
                                            .OrderBy(x => x.RequestDate)
                                            .Select(x => x.RequestDate)
                                            .FirstOrDefaultAsync();
                    nextRequest = firstDayOfMonth.AddMonths(1).Date;
                    break;
            }

            return new RequestQuotaCountModel()
            {
                HasLimit = true,
                Limit = limit,
                Remaining = count < 0 ? 0 : count,
                NextRequest = DateTime.SpecifyKind(nextRequest, DateTimeKind.Utc),
            };
        }
    }
}
