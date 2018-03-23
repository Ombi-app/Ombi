using Hangfire;
using Ombi.Core.Settings;
using Ombi.Schedule.Jobs;
using Ombi.Schedule.Jobs.Couchpotato;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Schedule.Jobs.Radarr;
using Ombi.Schedule.Jobs.SickRage;
using Ombi.Schedule.Jobs.Sonarr;
using Ombi.Settings.Settings.Models;

namespace Ombi.Schedule
{
    public class JobSetup : IJobSetup
    {
        public JobSetup(IPlexContentSync plexContentSync, IRadarrSync radarrSync,
            IOmbiAutomaticUpdater updater, IEmbyContentSync embySync, IPlexUserImporter userImporter,
            IEmbyUserImporter embyUserImporter, ISonarrSync cache, ICouchPotatoSync cpCache,
            ISettingsService<JobSettings> jobsettings, ISickRageSync srSync, IRefreshMetadata refresh,
            INewsletterJob newsletter)
        {
            _plexContentSync = plexContentSync;
            _radarrSync = radarrSync;
            _updater = updater;
            _embyContentSync = embySync;
            _plexUserImporter = userImporter;
            _embyUserImporter = embyUserImporter;
            _sonarrSync = cache;
            _cpCache = cpCache;
            _jobSettings = jobsettings;
            _srSync = srSync;
            _refreshMetadata = refresh;
            _newsletter = newsletter;
        }

        private readonly IPlexContentSync _plexContentSync;
        private readonly IRadarrSync _radarrSync;
        private readonly IOmbiAutomaticUpdater _updater;
        private readonly IPlexUserImporter _plexUserImporter;
        private readonly IEmbyContentSync _embyContentSync;
        private readonly IEmbyUserImporter _embyUserImporter;
        private readonly ISonarrSync _sonarrSync;
        private readonly ICouchPotatoSync _cpCache;
        private readonly ISickRageSync _srSync;
        private readonly ISettingsService<JobSettings> _jobSettings;
        private readonly IRefreshMetadata _refreshMetadata;
        private readonly INewsletterJob _newsletter;

        public void Setup()
        {
            var s = _jobSettings.GetSettings();

            RecurringJob.AddOrUpdate(() => _embyContentSync.Start(), JobSettingsHelper.EmbyContent(s));
            RecurringJob.AddOrUpdate(() => _sonarrSync.Start(), JobSettingsHelper.Sonarr(s));
            RecurringJob.AddOrUpdate(() => _radarrSync.CacheContent(), JobSettingsHelper.Radarr(s));
            RecurringJob.AddOrUpdate(() => _plexContentSync.CacheContent(), JobSettingsHelper.PlexContent(s));
            RecurringJob.AddOrUpdate(() => _cpCache.Start(), JobSettingsHelper.CouchPotato(s));
            RecurringJob.AddOrUpdate(() => _srSync.Start(), JobSettingsHelper.SickRageSync(s));
            RecurringJob.AddOrUpdate(() => _refreshMetadata.Start(), JobSettingsHelper.RefreshMetadata(s));

            RecurringJob.AddOrUpdate(() => _updater.Update(null), JobSettingsHelper.Updater(s));

            RecurringJob.AddOrUpdate(() => _embyUserImporter.Start(), JobSettingsHelper.UserImporter(s));
            RecurringJob.AddOrUpdate(() => _plexUserImporter.Start(), JobSettingsHelper.UserImporter(s));
            BackgroundJob.Enqueue(() => _newsletter.Start());
        }
    }
}
