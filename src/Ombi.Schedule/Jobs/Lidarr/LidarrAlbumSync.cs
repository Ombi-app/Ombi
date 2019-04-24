using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Ombi.Api.Lidarr;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Quartz;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Ombi.Schedule.Jobs.Lidarr
{
    public class LidarrAlbumSync : ILidarrAlbumSync
    {
        public LidarrAlbumSync(ISettingsService<LidarrSettings> lidarr, ILidarrApi lidarrApi, ILogger<LidarrAlbumSync> log, IExternalContext ctx,
            IBackgroundJobClient job, ILidarrAvailabilityChecker availability, IHubContext<NotificationHub> notification)
        {
            _lidarrSettings = lidarr;
            _lidarrApi = lidarrApi;
            _logger = log;
            _ctx = ctx;
            _job = job;
            _availability = availability;
            _notification = notification;
        }

        private readonly ISettingsService<LidarrSettings> _lidarrSettings;
        private readonly ILidarrApi _lidarrApi;
        private readonly ILogger _logger;
        private readonly IExternalContext _ctx;
        private readonly IBackgroundJobClient _job;
        private readonly ILidarrAvailabilityChecker _availability;
        private readonly IHubContext<NotificationHub> _notification;

        public async Task Execute(IJobExecutionContext ctx)
        {
            try
            {
                var settings = await _lidarrSettings.GetSettingsAsync();
                if (settings.Enabled)
                {

                    await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                        .SendAsync(NotificationHub.NotificationEvent, "Lidarr Album Sync Started");
                    try
                    {
                        var albums = await _lidarrApi.GetAllAlbums(settings.ApiKey, settings.FullUri);
                        if (albums != null && albums.Any())
                        {
                            // Let's remove the old cached data
                            await _ctx.Database.ExecuteSqlCommandAsync("DELETE FROM LidarrAlbumCache");

                            var albumCache = new List<LidarrAlbumCache>();
                            foreach (var a in albums)
                            {
                                if (a.id > 0)
                                {
                                    albumCache.Add(new LidarrAlbumCache
                                    {
                                        ArtistId = a.artistId,
                                        ForeignAlbumId = a.foreignAlbumId,
                                        ReleaseDate = a.releaseDate,
                                        TrackCount = a.currentRelease.trackCount,
                                        Monitored = a.monitored,
                                        Title = a.title,
                                        PercentOfTracks = a.statistics?.percentOfEpisodes ?? 0m,
                                        AddedAt = DateTime.Now,
                                    });
                                }
                            }
                            await _ctx.LidarrAlbumCache.AddRangeAsync(albumCache);

                            await _ctx.SaveChangesAsync();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                            .SendAsync(NotificationHub.NotificationEvent, "Lidarr Album Sync Failed");
                        _logger.LogError(LoggingEvents.Cacher, ex, "Failed caching queued items from Lidarr Album");
                    }

                    await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                        .SendAsync(NotificationHub.NotificationEvent, "Lidarr Album Sync Finished");

                    await OmbiQuartz.TriggerJob(nameof(ILidarrAvailabilityChecker), "DVR");
                }
            }
            catch (Exception)
            {
                _logger.LogInformation(LoggingEvents.LidarrArtistCache, "Lidarr is not setup, cannot cache Album");
            }
        }

        public async Task<IEnumerable<LidarrAlbumCache>> GetCachedContent()
        {
            return await _ctx.LidarrAlbumCache.ToListAsync();
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _ctx?.Dispose();
                _lidarrSettings?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}