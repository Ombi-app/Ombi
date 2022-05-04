#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: EmbyEpisodeCacher.cs
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
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ombi.Api.Emby;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Hubs;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Api.Emby.Models;
using Ombi.Api.Emby.Models.Media.Tv;

namespace Ombi.Schedule.Jobs.Emby
{
    public class EmbyEpisodeSync : IEmbyEpisodeSync
    {
        public EmbyEpisodeSync(ISettingsService<EmbySettings> s, IEmbyApiFactory api, ILogger<EmbyEpisodeSync> l, IEmbyContentRepository repo
            , IHubContext<NotificationHub> notification)
        {
            _apiFactory = api;
            _logger = l;
            _settings = s;
            _repo = repo;
            _notification = notification;
        }

        private readonly ISettingsService<EmbySettings> _settings;
        private readonly IEmbyApiFactory _apiFactory;
        private readonly ILogger<EmbyEpisodeSync> _logger;
        private readonly IEmbyContentRepository _repo;
        private readonly IHubContext<NotificationHub> _notification;

        private const int AmountToTake = 100;

        private IEmbyApi Api { get; set; }


        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.MergedJobDataMap;
            var recentlyAddedSearch = false;
            if (dataMap.TryGetValue(JobDataKeys.EmbyRecentlyAddedSearch, out var recentlyAddedObj))
            {
                recentlyAddedSearch = Convert.ToBoolean(recentlyAddedObj);
            }

            var settings = await _settings.GetSettingsAsync();

            Api = _apiFactory.CreateClient(settings);
            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Emby Episode Sync Started");
            foreach (var server in settings.Servers)
            {
                if (server.EmbySelectedLibraries.Any() && server.EmbySelectedLibraries.Any(x => x.Enabled))
                {
                    var tvLibsToFilter = server.EmbySelectedLibraries.Where(x => x.Enabled && x.CollectionType == "tvshows");
                    foreach (var tvParentIdFilter in tvLibsToFilter)
                    {
                        _logger.LogInformation($"Scanning Lib for episodes '{tvParentIdFilter.Title}'");
                        await CacheEpisodes(server, recentlyAddedSearch, tvParentIdFilter.Key);
                    }
                }
                else
                {
                    await CacheEpisodes(server, recentlyAddedSearch, string.Empty);
                }
            }

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Emby Episode Sync Finished");
            _logger.LogInformation("Emby Episode Sync Finished - Triggering Metadata refresh");
            await OmbiQuartz.TriggerJob(nameof(IRefreshMetadata), "System");
        }

        private async Task CacheEpisodes(EmbyServers server, bool recentlyAdded, string parentIdFilter)
        {
            EmbyItemContainer<EmbyEpisodes> allEpisodes;
            if (recentlyAdded)
            {
                var recentlyAddedAmountToTake = AmountToTake;
                allEpisodes = await Api.RecentlyAddedEpisodes(server.ApiKey, parentIdFilter, 0, recentlyAddedAmountToTake, server.AdministratorId, server.FullUri);
                if (allEpisodes.TotalRecordCount > recentlyAddedAmountToTake)
                {
                    allEpisodes.TotalRecordCount = recentlyAddedAmountToTake;
                }
            }
            else
            {
                allEpisodes = await Api.GetAllEpisodes(server.ApiKey, parentIdFilter, 0, AmountToTake, server.AdministratorId, server.FullUri);
            }
            var total = allEpisodes.TotalRecordCount;
            var processed = 0;
            var epToAdd = new HashSet<EmbyEpisode>();
            while (processed < total)
            {
                foreach (var ep in allEpisodes.Items)
                {
                    processed++;

                    // Let's make sure we have the parent request, stop those pesky forign key errors,
                    // Damn me having data integrity
                    var parent = await _repo.GetByEmbyId(ep.SeriesId);
                    if (parent == null)
                    {
                        _logger.LogInformation("The episode {0} does not relate to a series, so we cannot save this",
                            ep.Name);
                        continue;
                    }

                    var existingEpisode = await _repo.GetEpisodeByEmbyId(ep.Id);
                    // Make sure it's not in the hashset too
                    var existingInList = epToAdd.Any(x => x.EmbyId == ep.Id);

                    if (existingEpisode == null && !existingInList)
                    {
                        // Sanity checks
                        if (ep.IndexNumber == 0)
                        {
                            _logger.LogWarning($"Episode {ep.Name} has no episode number. Skipping.");
                            continue;
                        }

                        _logger.LogDebug("Adding new episode {0} to parent {1}", ep.Name, ep.SeriesName);
                        // add it
                        epToAdd.Add(new EmbyEpisode
                        {
                            EmbyId = ep.Id,
                            EpisodeNumber = ep.IndexNumber,
                            SeasonNumber = ep.ParentIndexNumber,
                            ParentId = ep.SeriesId,
                            TvDbId = ep.ProviderIds.Tvdb,
                            TheMovieDbId = ep.ProviderIds.Tmdb,
                            ImdbId = ep.ProviderIds.Imdb,
                            Title = ep.Name,
                            AddedAt = DateTime.UtcNow
                        });

                        if (ep.IndexNumberEnd.HasValue && ep.IndexNumberEnd.Value != ep.IndexNumber)
                        {
                            int episodeNumber = ep.IndexNumber;
                            do
                            {
                                _logger.LogDebug($"Multiple-episode file detected. Adding episode ${episodeNumber}");
                                episodeNumber++;
                                epToAdd.Add(new EmbyEpisode
                                {
                                    EmbyId = ep.Id,
                                    EpisodeNumber = episodeNumber,
                                    SeasonNumber = ep.ParentIndexNumber,
                                    ParentId = ep.SeriesId,
                                    TvDbId = ep.ProviderIds.Tvdb,
                                    TheMovieDbId = ep.ProviderIds.Tmdb,
                                    ImdbId = ep.ProviderIds.Imdb,
                                    Title = ep.Name,
                                    AddedAt = DateTime.UtcNow
                                });

                            } while (episodeNumber < ep.IndexNumberEnd.Value);
                        
                        }
                    }
                }

                await _repo.AddRange(epToAdd);
                epToAdd.Clear();
                if (!recentlyAdded)
                {
                    allEpisodes = await Api.GetAllEpisodes(server.ApiKey, parentIdFilter, processed, AmountToTake, server.AdministratorId, server.FullUri);
                }
            }

            if (epToAdd.Any())
            {
                await _repo.AddRange(epToAdd);
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
