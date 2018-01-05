﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Sonarr;
using Ombi.Api.Sonarr.Models;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Schedule.Jobs.Sonarr
{
    public class SonarrSync : ISonarrSync
    {
        public SonarrSync(ISettingsService<SonarrSettings> s, ISonarrApi api, ILogger<SonarrSync> l, IOmbiContext ctx)
        {
            _settings = s;
            _api = api;
            _log = l;
            _ctx = ctx;
            _settings.ClearCache();
        }

        private readonly ISettingsService<SonarrSettings> _settings;
        private readonly ISonarrApi _api;
        private readonly ILogger<SonarrSync> _log;
        private readonly IOmbiContext _ctx;

        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
        public async Task Start()
        {
            await SemaphoreSlim.WaitAsync();
            try
            {
                var settings = await _settings.GetSettingsAsync();
                if (!settings.Enabled)
                {
                    return;
                }
                var series = await _api.GetSeries(settings.ApiKey, settings.FullUri);
                if (series != null)
                {
                    var sonarrSeries = series as IList<SonarrSeries> ?? series.ToList();
                    var ids = sonarrSeries.Select(x => x.tvdbId);

                    await _ctx.Database.ExecuteSqlCommandAsync("DELETE FROM SonarrCache");
                    var entites = ids.Select(id => new SonarrCache { TvDbId = id }).ToList();

                    await _ctx.SonarrCache.AddRangeAsync(entites);
                    
                    await _ctx.Database.ExecuteSqlCommandAsync("DELETE FROM SonarrEpisodeCache");
                    foreach (var s in sonarrSeries)
                    {
                        _log.LogDebug("Syncing series: {0}", s.title);
                        var episodes = await _api.GetEpisodes(s.id, settings.ApiKey, settings.FullUri);
                        var monitoredEpisodes = episodes.Where(x => x.monitored || x.hasFile);
                        
                        // Add to DB
                        _log.LogDebug("We have the episodes, adding to db transaction");
                        await _ctx.SonarrEpisodeCache.AddRangeAsync(monitoredEpisodes.Select(episode => new SonarrEpisodeCache
                        {
                            EpisodeNumber = episode.episodeNumber,
                            SeasonNumber = episode.seasonNumber,
                            TvDbId = s.tvdbId,
                            HasFile = episode.hasFile
                        }));
                    }
                    
                    _log.LogDebug("Commiting the transaction");
                    await _ctx.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _log.LogError(LoggingEvents.SonarrCacher, e, "Exception when trying to cache Sonarr");
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }
    }
}