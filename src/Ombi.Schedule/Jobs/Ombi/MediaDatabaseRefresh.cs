using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Jellyfin;
using Ombi.Schedule.Jobs.Plex.Interfaces;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class MediaDatabaseRefresh : IMediaDatabaseRefresh
    {
        public MediaDatabaseRefresh(ISettingsService<PlexSettings> s, ILogger<MediaDatabaseRefresh> log,
            IPlexContentRepository plexRepo, IEmbyContentRepository embyRepo, IJellyfinContentRepository jellyfinRepo)
        {
            _settings = s;
            _log = log;
            _plexRepo = plexRepo;
            _embyRepo = embyRepo;
            _jellyfinRepo = jellyfinRepo;
            _settings.ClearCache();
        }

        private readonly ISettingsService<PlexSettings> _settings;
        private readonly ILogger _log;
        private readonly IPlexContentRepository _plexRepo;
        private readonly IEmbyContentRepository _embyRepo;
        private readonly IJellyfinContentRepository _jellyfinRepo;

        public async Task Execute(IJobExecutionContext job)
        {
            try
            {
                await RemovePlexData();
                await RemoveEmbyData();
                await RemoveJellyfinData();
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

                await OmbiQuartz.TriggerJob(nameof(IEmbyContentSync), "Emby");
            }
            catch (Exception e)
            {
                _log.LogError(LoggingEvents.MediaReferesh, e, "Refreshing Emby Data Failed");
            }
        }

        private async Task RemoveJellyfinData()
        {
            try
            {
                var s = await _settings.GetSettingsAsync();
                if (!s.Enable)
                {
                    return;
                }
                const string episodeSQL = "DELETE FROM JellyfinEpisode";
                const string mainSql = "DELETE FROM JellyfinContent";
                await _jellyfinRepo.ExecuteSql(episodeSQL);
                await _jellyfinRepo.ExecuteSql(mainSql);

                await OmbiQuartz.TriggerJob(nameof(IJellyfinContentSync), "Jellyfin");
            }
            catch (Exception e)
            {
                _log.LogError(LoggingEvents.MediaReferesh, e, "Refreshing Jellyfin Data Failed");
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
                //_settings?.Dispose();
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
