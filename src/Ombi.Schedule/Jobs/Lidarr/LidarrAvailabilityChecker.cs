using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core.Notifications;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Schedule.Jobs.Lidarr
{
    public class LidarrAvailabilityChecker : ILidarrAvailabilityChecker
    {
        public LidarrAvailabilityChecker(IMusicRequestRepository requests, IRepository<LidarrAlbumCache> albums, ILogger<LidarrAvailabilityChecker> log,
            IBackgroundJobClient job, INotificationService notification)
        {
            _cachedAlbums = albums;
            _requestRepository = requests;
            _logger = log;
            _job = job;
            _notificationService = notification;
        }

        private readonly IMusicRequestRepository _requestRepository;
        private readonly IRepository<LidarrAlbumCache> _cachedAlbums;
        private readonly ILogger _logger;
        private readonly IBackgroundJobClient _job;
        private readonly INotificationService _notificationService;

        public async Task Start()
        {
            var allAlbumRequests = _requestRepository.GetAll().Include(x => x.RequestedUser).Where(x => !x.Available);
            var albumsToUpdate = new List<AlbumRequest>();
            foreach (var request in allAlbumRequests)
            {
                // Check if we have it cached
                var cachedAlbum = await _cachedAlbums.FirstOrDefaultAsync(x => x.ForeignAlbumId.Equals(request.ForeignAlbumId));
                if (cachedAlbum != null)
                {
                    if (cachedAlbum.FullyAvailable)
                    {
                        request.Available = true;
                        request.MarkedAsAvailable = DateTime.Now;
                        albumsToUpdate.Add(request);
                    }
                }
            }

            foreach (var albumRequest in albumsToUpdate)
            {
                await _requestRepository.Update(albumRequest);
                var recipient = albumRequest.RequestedUser.Email.HasValue() ? albumRequest.RequestedUser.Email : string.Empty;

                _logger.LogDebug("AlbumId: {0}, RequestUser: {1}", albumRequest.Id, recipient);

                _job.Enqueue(() => _notificationService.Publish(new NotificationOptions
                {
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.RequestAvailable,
                    RequestId = albumRequest.Id,
                    RequestType = RequestType.Album,
                    Recipient = recipient,
                }));
            }
        }
    }
}
