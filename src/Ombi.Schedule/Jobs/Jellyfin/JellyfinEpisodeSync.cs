#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: JellyfinEpisodeCacher.cs
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
using Ombi.Api.Jellyfin;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Hubs;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Schedule.Jobs.MediaServer;
using Ombi.Api.Jellyfin.Models.Media.Tv;

namespace Ombi.Schedule.Jobs.Jellyfin
{
    public class JellyfinEpisodeSync : MediaServerEpisodeSync<JellyfinEpisodes, JellyfinEpisode, IJellyfinContentRepository, JellyfinContent>, IJellyfinEpisodeSync
    {


        public JellyfinEpisodeSync(
            ISettingsService<JellyfinSettings> s,
            IJellyfinApiFactory api,
            ILogger<MediaServerEpisodeSync<JellyfinEpisodes, JellyfinEpisode, IJellyfinContentRepository, JellyfinContent>> l,
            IJellyfinContentRepository repo,
            IHubContext<NotificationHub> notification) : base(l, repo, notification)
        {
            _apiFactory = api;
            _settings = s;
        }
        private readonly ISettingsService<JellyfinSettings> _settings;
        private readonly IJellyfinApiFactory _apiFactory;
        private IJellyfinApi Api { get; set; }

        public override async Task Execute(IJobExecutionContext job)
        {
            var settings = await _settings.GetSettingsAsync();

            Api = _apiFactory.CreateClient(settings);
            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Jellyfin Episode Sync Started");

            await CacheEpisodes();

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Jellyfin Episode Sync Finished");
            _logger.LogInformation("Jellyfin Episode Sync Finished - Triggering Metadata refresh");
            await OmbiQuartz.TriggerJob(nameof(IRefreshMetadata), "System");
        }

        private int packageSize = 200;
        protected override async IAsyncEnumerable<JellyfinEpisodes> GetMediaServerEpisodes()
        {
            var settings = await _settings.GetSettingsAsync();
            Api = _apiFactory.CreateClient(settings);

            foreach (var server in settings.Servers)
            {

                if (server.JellyfinSelectedLibraries.Any() && server.JellyfinSelectedLibraries.Any(x => x.Enabled))
                {
                    var tvLibsToFilter = server.JellyfinSelectedLibraries.Where(x => x.Enabled && x.CollectionType == "tvshows");
                    foreach (var tvParentIdFilter in tvLibsToFilter)
                    {
                        _logger.LogInformation($"Scanning Lib for episodes '{tvParentIdFilter.Title}'");
                        await foreach (var ep in GetEpisodesFromLibrary(server, tvParentIdFilter.Key))
                        {
                            yield return ep;
                        }
                    }
                }
                else
                {
                    await foreach (var ep in GetEpisodesFromLibrary(server, string.Empty))
                    {
                        yield return ep;
                    }
                }
            }
        }
        private async IAsyncEnumerable<JellyfinEpisodes> GetEpisodesFromLibrary(JellyfinServers server, string parentIdFilter)
        {
            var processed = 0;
            var allEpisodes = await Api.GetAllEpisodes(server.ApiKey, parentIdFilter, processed, packageSize, server.AdministratorId, server.FullUri);

            var total = allEpisodes.TotalRecordCount;
            while (processed < total)
            {
                foreach (var ep in allEpisodes.Items)
                {
                    processed++;

                    if (ep.LocationType?.Equals("Virtual", StringComparison.InvariantCultureIgnoreCase) ?? false)
                    {
                        // For some reason Jellyfin is not respecting the `IsVirtualItem` field.
                        continue;
                    }

                    // Let's make sure we have the parent request, stop those pesky forign key errors,
                    // Damn me having data integrity
                    var parent = await _repo.GetByJellyfinId(ep.SeriesId);
                    if (parent == null)
                    {
                        _logger.LogInformation("The episode {0} does not relate to a series, so we cannot save this",
                            ep.Name);
                        continue;
                    }
                    yield return ep;
                }
                allEpisodes = await Api.GetAllEpisodes(server.ApiKey, parentIdFilter, processed, packageSize, server.AdministratorId, server.FullUri);
            }
        }

        protected override void addEpisode(JellyfinEpisodes ep, ICollection<JellyfinEpisode> epToAdd)
        {

            _logger.LogDebug("Adding new episode {0} to parent {1}", ep.Name, ep.SeriesName);
            // add it
            epToAdd.Add(new JellyfinEpisode
            {
                JellyfinId = ep.Id,
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
                epToAdd.Add(new JellyfinEpisode
                {
                    JellyfinId = ep.Id,
                    EpisodeNumber = ep.IndexNumberEnd.Value,
                    SeasonNumber = ep.ParentIndexNumber,
                    ParentId = ep.SeriesId,
                    TvDbId = ep.ProviderIds.Tvdb,
                    TheMovieDbId = ep.ProviderIds.Tmdb,
                    ImdbId = ep.ProviderIds.Imdb,
                    Title = ep.Name,
                    AddedAt = DateTime.UtcNow
                });
            }
        }

        protected override async Task<JellyfinEpisode> GetExistingEpisode(JellyfinEpisodes ep)
        {
            return await _repo.GetEpisodeByJellyfinId(ep.Id);
        }

        protected override bool IsIn(JellyfinEpisodes ep, ICollection<JellyfinEpisode> list)
        {
            return list.Any(x => x.JellyfinId == ep.Id);
        }
    }
}
