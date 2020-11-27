using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Core.Engine
{
    public class UserDeletionEngine : IUserDeletionEngine
    {
        private readonly IMovieRequestRepository _movieRepository;
        private readonly OmbiUserManager _userManager;
        private readonly IRepository<Issues> _issuesRepository;
        private readonly IRepository<IssueComments> _issueCommentsRepository;
        private readonly IRepository<RequestLog> _requestLogRepository;
        private readonly IRepository<NotificationUserId> _notificationRepository;
        private readonly IRepository<RequestSubscription> _requestSubscriptionRepository;
        private readonly IRepository<UserNotificationPreferences> _userNotificationPreferences;
        private readonly IRepository<UserQualityProfiles> _userQualityProfiles;
        private readonly ITvRequestRepository _tvRepository;
        private readonly IMusicRequestRepository _musicRepository;
        private readonly IRepository<Votes> _voteRepository;
        private readonly IRepository<MobileDevices> _mobileDevicesRepository;

        public UserDeletionEngine(IMovieRequestRepository movieRepository,
                                    OmbiUserManager userManager,
                                    ITvRequestRepository tvRepository,
                                    IMusicRequestRepository musicRepository,
                                    IRepository<Issues> issueRepo,
                                    IRepository<IssueComments> issueCommentsRepo,
                                    IRepository<RequestLog> requestLogRepo,
                                    IRepository<NotificationUserId> notificationidsRepo,
                                    IRepository<RequestSubscription> requestSubRepository,
                                    IRepository<UserNotificationPreferences> notificationPreferencesRepo,
                                    IRepository<UserQualityProfiles> qualityProfilesRepo,
                                    IRepository<Votes> voteRepository,
                                    IRepository<MobileDevices> mobileDevicesRepository
                                    )
        {
            _movieRepository = movieRepository;
            _userManager = userManager;
            _tvRepository = tvRepository;
            _musicRepository = musicRepository;
            _issuesRepository = issueRepo;
            _issueCommentsRepository = issueCommentsRepo;
            _notificationRepository = notificationidsRepo;
            _requestLogRepository = requestLogRepo;
            _requestSubscriptionRepository = requestSubRepository;
            _notificationRepository = notificationidsRepo;
            _userNotificationPreferences = notificationPreferencesRepo;
            _userQualityProfiles = qualityProfilesRepo;
            _voteRepository = voteRepository;
            _mobileDevicesRepository = mobileDevicesRepository;
        }


        public async Task<IdentityResult> DeleteUser(OmbiUser userToDelete)
        {
            var userId = userToDelete.Id;
            // We need to delete all the requests first
            var moviesUserRequested = _movieRepository.GetAll().Where(x => x.RequestedUserId == userId);
            var tvUserRequested = _tvRepository.GetChild().Where(x => x.RequestedUserId == userId);
            var musicRequested = _musicRepository.GetAll().Where(x => x.RequestedUserId == userId);
            var notificationPreferences = _userNotificationPreferences.GetAll().Where(x => x.UserId == userId);
            var userQuality = await _userQualityProfiles.GetAll().FirstOrDefaultAsync(x => x.UserId == userId);

            if (moviesUserRequested.Any())
            {
                await _movieRepository.DeleteRange(moviesUserRequested);
            }
            if (tvUserRequested.Any())
            {
                await _tvRepository.DeleteChildRange(tvUserRequested);
            }
            if (musicRequested.Any())
            {
                await _musicRepository.DeleteRange(musicRequested);
            }
            if (notificationPreferences.Any())
            {
                await _userNotificationPreferences.DeleteRange(notificationPreferences);
            }
            if (userQuality != null)
            {
                await _userQualityProfiles.Delete(userQuality);
            }

            // Delete any issues and request logs
            var issues = _issuesRepository.GetAll().Where(x => x.UserReportedId == userId);
            var issueComments = _issueCommentsRepository.GetAll().Where(x => x.UserId == userId);
            var requestLog = _requestLogRepository.GetAll().Where(x => x.UserId == userId);
            if (issues.Any())
            {
                await _issuesRepository.DeleteRange(issues);
            }
            if (requestLog.Any())
            {
                await _requestLogRepository.DeleteRange(requestLog);
            }
            if (issueComments.Any())
            {
                await _issueCommentsRepository.DeleteRange(issueComments);
            }

            // Delete the Subscriptions and mobile notification ids
            var subs = _requestSubscriptionRepository.GetAll().Where(x => x.UserId == userId);
            var mobileIds = _notificationRepository.GetAll().Where(x => x.UserId == userId);
            var votes = _voteRepository.GetAll().Where(x => x.UserId == userId);
            var newMobiles = _mobileDevicesRepository.GetAll().Where(x => x.UserId == userId);
            if (subs.Any())
            {
                await _requestSubscriptionRepository.DeleteRange(subs);
            }
            if (mobileIds.Any())
            {
                await _notificationRepository.DeleteRange(mobileIds);
            }
            if (votes.Any())
            {
                await _voteRepository.DeleteRange(votes);
            }
            if (newMobiles.Any())
            {
                await _mobileDevicesRepository.DeleteRange(newMobiles);
            }

            var result = await _userManager.DeleteAsync(userToDelete);
            return result;
        }
    }
}
