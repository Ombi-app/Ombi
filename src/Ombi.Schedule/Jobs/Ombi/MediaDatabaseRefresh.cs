using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Plex.Interfaces;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class MediaDatabaseRefresh : IMediaDatabaseRefresh
    {
        public MediaDatabaseRefresh(ISettingsService<PlexSettings> s, ILogger<MediaDatabaseRefresh> log,
            IPlexContentRepository plexRepo, IEmbyContentRepository embyRepo, IEmbyContentSync embySync)
        {
            _settings = s;
            _log = log;
            _plexRepo = plexRepo;
            _embyRepo = embyRepo;
            _embyContentSync = embySync;
        }

        private readonly ISettingsService<PlexSettings> _settings;
        private readonly ILogger _log;
        private readonly IPlexContentRepository _plexRepo;
        private readonly IEmbyContentRepository _embyRepo;
        private readonly IEmbyContentSync _embyContentSync;

        public async Task Start()
        {
            try
            {
                await RemovePlexData();
                await RemoveEmbyData();
            }
            catch (Exception e)
            {
                _log.LogError(LoggingEvents.MediaReferesh, e, "Refreshing Media Data Failed");
            }

        }

        private async Task RemoveEmbyData()
        {
            try
            {
                var s = await _settings.GetSettingsAsync();
                if (!s.Enable)
                {
                    return;
                }

                const string episodeSQL = "DELETE FROM EmbyEpisode";
                const string mainSql = "DELETE FROM EmbyContent";
                await _embyRepo.ExecuteSql(episodeSQL);
                await _embyRepo.ExecuteSql(mainSql);

                BackgroundJob.Enqueue(() => _embyContentSync.Start());
            }
            catch (Exception e)
            {
                _log.LogError(LoggingEvents.MediaReferesh, e, "Refreshing Emby Data Failed");
            }
        }

        private async Task RemovePlexData()
        {
            try
            {
                var s = await _settings.GetSettingsAsync();
                if (!s.Enable)
                {
                    return;
                }

                const string episodeSQL = "DELETE FROM PlexEpisode";
                const string seasonsSql = "DELETE FROM PlexSeasonsContent";
                const string mainSql = "DELETE FROM PlexServerContent";
                await _plexRepo.ExecuteSql(episodeSQL);
                await _plexRepo.ExecuteSql(seasonsSql);
                await _plexRepo.ExecuteSql(mainSql);
            }
            catch (Exception e)
            {
                _log.LogError(LoggingEvents.MediaReferesh, e, "Refreshing Plex Data Failed");
            }
        }



        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _plexRepo?.Dispose();
                _settings?.Dispose();
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