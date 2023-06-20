using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Emby;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Quartz;

namespace Ombi.Schedule.Jobs.Emby
{
    public abstract class EmbyLibrarySync
    {
        public EmbyLibrarySync(ISettingsService<EmbySettings> settings, IEmbyApiFactory api, ILogger<EmbyContentSync> logger,
            INotificationHubService notification)
        {
            _logger = logger;
            _settings = settings;
            _apiFactory = api;
            _notification = notification;
        }

        protected readonly ILogger<EmbyContentSync> _logger;
        protected readonly ISettingsService<EmbySettings> _settings;
        protected readonly IEmbyApiFactory _apiFactory;
        protected bool recentlyAdded;
        protected readonly INotificationHubService _notification;

        protected const int AmountToTake = 100;

        protected IEmbyApi Api { get; set; }

        public virtual async Task Execute(IJobExecutionContext context)
        {
            
            JobDataMap dataMap = context.MergedJobDataMap;
            if (dataMap.TryGetValue(JobDataKeys.EmbyRecentlyAddedSearch, out var recentlyAddedObj))
            {
                recentlyAdded = Convert.ToBoolean(recentlyAddedObj);
            }

            await _notification.SendNotificationToAdmins(recentlyAdded ? "Emby Recently Added Started" : "Emby Content Sync Started");


            var embySettings = await _settings.GetSettingsAsync();
            if (!embySettings.Enable)
                return;

            Api = _apiFactory.CreateClient(embySettings);

            foreach (var server in embySettings.Servers)
            {
                try
                {
                    await StartServerCache(server);
                }
                catch (Exception e)
                {
                    await _notification.SendNotificationToAdmins("Emby Content Sync Failed");
                    _logger.LogError(e, "Exception when caching Emby for server {0}", server.Name);
                }
            }

            await _notification.SendNotificationToAdmins("Emby Content Sync Finished");
        }


        private async Task StartServerCache(EmbyServers server)
        {
            if (!ValidateSettings(server))
            {
                return;
            }


            if (server.EmbySelectedLibraries.Any() && server.EmbySelectedLibraries.Any(x => x.Enabled))
            {
                var movieLibsToFilter = server.EmbySelectedLibraries.Where(x => x.Enabled && x.CollectionType == "movies");

                foreach (var movieParentIdFilder in movieLibsToFilter)
                {
                    _logger.LogInformation($"Scanning Lib '{movieParentIdFilder.Title}'");
                    await ProcessMovies(server, movieParentIdFilder.Key);
                }

                var tvLibsToFilter = server.EmbySelectedLibraries.Where(x => x.Enabled && x.CollectionType == "tvshows");
                foreach (var tvParentIdFilter in tvLibsToFilter)
                {
                    _logger.LogInformation($"Scanning Lib '{tvParentIdFilter.Title}'");
                    await ProcessTv(server, tvParentIdFilter.Key);
                }


                var mixedLibs = server.EmbySelectedLibraries.Where(x => x.Enabled && x.CollectionType == "mixed");
                foreach (var m in mixedLibs)
                {
                    _logger.LogInformation($"Scanning Lib '{m.Title}'");
                    await ProcessTv(server, m.Key);
                    await ProcessMovies(server, m.Key);
                }
            }
            else
            {
                await ProcessMovies(server);
                await ProcessTv(server);
            }
        }

        protected abstract Task ProcessTv(EmbyServers server, string parentId = default);

        protected abstract Task ProcessMovies(EmbyServers server, string parentId = default);

        private bool ValidateSettings(EmbyServers server)
        {
            if (server?.Ip == null || string.IsNullOrEmpty(server?.ApiKey))
            {
                _logger.LogInformation(LoggingEvents.EmbyContentCacher, $"Server {server?.Name} is not configured correctly");
                return false;
            }

            return true;
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
