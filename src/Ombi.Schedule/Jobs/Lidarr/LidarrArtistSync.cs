using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Ombi.Api.Lidarr;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Ombi.Schedule.Jobs.Lidarr
{
    public class LidarrArtistSync : ILidarrArtistSync
    {
        public LidarrArtistSync(ISettingsService<LidarrSettings> lidarr, ILidarrApi lidarrApi, ILogger<LidarrArtistSync> log, IExternalContext ctx,
            IBackgroundJobClient background, ILidarrAlbumSync album)
        {
            _lidarrSettings = lidarr;
            _lidarrApi = lidarrApi;
            _logger = log;
            _ctx = ctx;
            _job = background;
            _albumSync = album;
        }

        private readonly ISettingsService<LidarrSettings> _lidarrSettings;
        private readonly ILidarrApi _lidarrApi;
        private readonly ILogger _logger;
        private readonly IExternalContext _ctx;
        private readonly IBackgroundJobClient _job;
        private readonly ILidarrAlbumSync _albumSync;
        
        public async Task CacheContent()
        {
            try
            {
                var settings = await _lidarrSettings.GetSettingsAsync();
                if (settings.Enabled)
                {
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
                        _logger.LogError(LoggingEvents.Cacher, ex, "Failed caching queued items from Lidarr");
                    }

                    _job.Enqueue(() => _albumSync.CacheContent());
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