﻿using System;
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
    public class LidarrArtistSync : ILidarrArtistSync
    {
        public LidarrArtistSync(ISettingsService<LidarrSettings> lidarr, ILidarrApi lidarrApi, ILogger<LidarrArtistSync> log, IExternalContext ctx
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
        private readonly IExternalContext _ctx;
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
                            // Let's remove the old cached data
                            await _ctx.Database.ExecuteSqlCommandAsync("DELETE FROM LidarrArtistCache");

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
                            await _ctx.LidarrArtistCache.AddRangeAsync(artistCache);

                            await _ctx.SaveChangesAsync();
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