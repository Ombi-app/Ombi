#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: PlexServerContentCacher.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Quartz;

namespace Ombi.Schedule.Jobs.Plex
{
    public abstract class PlexLibrarySync
    {
        
        public PlexLibrarySync(
            ISettingsService<PlexSettings> plex,
            IPlexApi plexApi,
            ILogger<PlexLibrarySync> logger,
            INotificationHubService notificationHubService)
        {
            PlexApi = plexApi;
            Plex = plex;
            Logger = logger;
            Notification = notificationHubService;
        }
        protected ILogger<PlexLibrarySync> Logger { get; }
        protected IPlexApi PlexApi { get; }
        protected ISettingsService<PlexSettings> Plex { get; }
        private INotificationHubService Notification { get; set; }
        protected bool recentlyAddedSearch;
        public virtual async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.MergedJobDataMap;
            recentlyAddedSearch = dataMap.GetBooleanValueFromString(JobDataKeys.RecentlyAddedSearch);

            var plexSettings = await Plex.GetSettingsAsync();
            if (!plexSettings.Enable)
            {
                return;
            }
            await NotifyClient(recentlyAddedSearch ? "Plex Recently Added Sync Started" : "Plex Content Sync Started");
            if (!ValidateSettings(plexSettings))
            {
                Logger.LogError("Plex Settings are not valid");
                await NotifyClient(recentlyAddedSearch ? "Plex Recently Added Sync, Settings Not Valid" : "Plex Content, Settings Not Valid");
                return;
            }
            Logger.LogInformation(recentlyAddedSearch
                ? "Starting Plex Content Cacher Recently Added Scan"
                : "Starting Plex Content Cacher");
            try
            {
                await StartTheCache(plexSettings);
            }
            catch (Exception e)
            {
                await NotifyClient(recentlyAddedSearch ? "Plex Recently Added Sync Errored" : "Plex Content Sync Errored");
                Logger.LogWarning(LoggingEvents.PlexContentCacher, e, "Exception thrown when attempting to cache the Plex Content");
            }
        }
        
        private async Task StartTheCache(PlexSettings plexSettings)
        {
            foreach (var servers in plexSettings.Servers ?? new List<PlexServers>())
            {
                try
                {
                    Logger.LogInformation("Starting to cache the content on server {0}", servers.Name);
                    await ProcessServer(servers);
                }
                catch (Exception e)
                {
                    Logger.LogWarning(LoggingEvents.PlexContentCacher, e, "Exception thrown when attempting to cache the Plex Content in server {0}", servers.Name);
                }
            }
        }

        protected async Task<List<Directory>> GetEnabledLibraries(PlexServers plexSettings)
        {
            var result = new List<Directory>();
            var sections = await PlexApi.GetLibrarySections(plexSettings.PlexAuthToken, plexSettings.FullUri);

            if (sections != null)
            {
                foreach (var dir in sections.MediaContainer.Directory ?? new List<Directory>())
                {
                    if (plexSettings.PlexSelectedLibraries.Any())
                    {
                        if (plexSettings.PlexSelectedLibraries.Any(x => x.Enabled))
                        {
                            // Only get the enabled libs
                            var keys = plexSettings.PlexSelectedLibraries.Where(x => x.Enabled)
                                .Select(x => x.Key.ToString()).ToList();
                            if (!keys.Contains(dir.key))
                            {
                                Logger.LogDebug("Lib {0} is not monitored, so skipping", dir.key);
                                // We are not monitoring this lib
                                continue;
                            }
                        }
                    }
                    result.Add(dir);

                }
            }

            return result;
        }
        
        protected abstract Task ProcessServer(PlexServers servers);

        protected async Task NotifyClient(string message)
        {
            await Notification.SendNotificationToAdmins($"Plex Sync - {message}");
        }
        private static bool ValidateSettings(PlexSettings plex)
        {
            if (plex.Enable)
            {
                foreach (var server in plex.Servers ?? new List<PlexServers>())
                {
                    if (string.IsNullOrEmpty(server?.Ip) || string.IsNullOrEmpty(server?.PlexAuthToken))
                    {
                        return false;
                    }
                }
            }
            return plex.Enable;
        }

    }
}