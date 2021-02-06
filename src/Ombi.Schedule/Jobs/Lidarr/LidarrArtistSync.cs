using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
    public class LidarrArtistSync : ILidarrArtistSync
    {
        public LidarrArtistSync(ISettingsService<LidarrSettings> lidarr, ILidarrApi lidarrApi, ILogger<LidarrArtistSync> log, ExternalContext ctx
            , IHubContext<NotificationHub> notification)
        {
            _lidarrSettings = lidarr;
            _lidarrApi = lidarrApi;
            _logger = log;
            _ctx = ctx;
            _notification = notification;
        }

        private readonly ISettingsService<LidarrSettings> _lidarrSettings;
        private readonly ILidarrApi _lidarrApi;
        private readonly ILogger _logger;
        private readonly ExternalContext _ctx;
        private readonly IHubContext<NotificationHub> _notification;

        public async Task Execute(IJobExecutionContext job)
        {
            try
            {
                var settings = await _lidarrSettings.GetSettingsAsync();
                if (settings.Enabled)
                {

                    await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                        .SendAsync(NotificationHub.NotificationEvent, "Lidarr Artist Sync Started");
                    try
                    {
                        var artists = await _lidarrApi.GetArtists(settings.ApiKey, settings.FullUri);
                        if (artists != null && artists.Any())
                        {
                            var strat = _ctx.Database.CreateExecutionStrategy();
                            await strat.ExecuteAsync(async () =>
                            {
                                // Let's remove the old cached data
                                using (var tran = await _ctx.Database.BeginTransactionAsync())
                                {
                                    await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM LidarrArtistCache");
                                    tran.Commit();
                                }
                            });

                            var artistCache = new List<LidarrArtistCache>();
                            foreach (var a in artists)
                            {
                                if (a.id > 0)
                                {
                                    artistCache.Add(new LidarrArtistCache
                                    {
                                        ArtistId = a.id,
                                        ArtistName = a.artistName,
                                        ForeignArtistId = a.foreignArtistId,
                                        Monitored = a.monitored
                                    });
                                }
                            }
                            strat = _ctx.Database.CreateExecutionStrategy();
                            await strat.ExecuteAsync(async () =>
                            {
                                using (var tran = await _ctx.Database.BeginTransactionAsync())
                                {
                                    await _ctx.LidarrArtistCache.AddRangeAsync(artistCache);

                                    await _ctx.SaveChangesAsync();
                                    tran.Commit();
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                            .SendAsync(NotificationHub.NotificationEvent, "Lidarr Artist Sync Failed");
                        _logger.LogError(LoggingEvents.Cacher, ex, "Failed caching queued items from Lidarr");
                    }

                    await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                        .SendAsync(NotificationHub.NotificationEvent, "Lidarr Artist Sync Finished");

                    await OmbiQuartz.TriggerJob(nameof(ILidarrAlbumSync), "DVR");
                }
            }
            catch (Exception)
            {
                _logger.LogInformation(LoggingEvents.LidarrArtistCache, "Lidarr is not setup, cannot cache Artist");
            }
        }

        public async Task<IEnumerable<LidarrArtistCache>> GetCachedContent()
        {
            return await _ctx.LidarrArtistCache.ToListAsync();
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _ctx?.Dispose();
                //_lidarrSettings?.Dispose();
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