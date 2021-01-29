using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Sonarr;
using Ombi.Api.Sonarr.Models;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Schedule.Jobs.Radarr;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Quartz;

namespace Ombi.Schedule.Jobs.Sonarr
{
    public class SonarrSync : ISonarrSync
    {
        public SonarrSync(ISettingsService<SonarrSettings> s, ISonarrApi api, ILogger<SonarrSync> l, ExternalContext ctx)
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
        private readonly ExternalContext _ctx;

        public async Task Execute(IJobExecutionContext job)
        {
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
                    var sonarrSeries = series as ImmutableHashSet<SonarrSeries> ?? series.ToImmutableHashSet();
                    var ids = sonarrSeries.Select(x => x.tvdbId); 
                    var strat = _ctx.Database.CreateExecutionStrategy();
                    await strat.ExecuteAsync(async () =>
                    {
                        using (var tran = await _ctx.Database.BeginTransactionAsync())
                        {
                            await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM SonarrCache");
                            tran.Commit();
                        }
                    });

                    var existingSeries = await _ctx.SonarrCache.Select(x => x.TvDbId).ToListAsync();
                    
                    //var entites = ids.Except(existingSeries).Select(id => new SonarrCache { TvDbId = id }).ToImmutableHashSet();
                    var entites = ids.Select(id => new SonarrCache { TvDbId = id }).ToImmutableHashSet();

                    await _ctx.SonarrCache.AddRangeAsync(entites);
                    entites.Clear();
                    strat = _ctx.Database.CreateExecutionStrategy();
                    await strat.ExecuteAsync(async () =>
                    {
                        using (var tran = await _ctx.Database.BeginTransactionAsync())
                        {
                            await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM SonarrEpisodeCache");
                            tran.Commit();
                        }
                    });

                    foreach (var s in sonarrSeries)
                    {
                        if (!s.monitored || s.episodeFileCount == 0) // We have files
                        {
                            continue;
                        }

                        _log.LogDebug("Syncing series: {0}", s.title);
                        var episodes = await _api.GetEpisodes(s.id, settings.ApiKey, settings.FullUri);
                        var monitoredEpisodes = episodes.Where(x => x.monitored || x.hasFile);

                        //var allExistingEpisodes = await _ctx.SonarrEpisodeCache.Where(x => x.TvDbId == s.tvdbId).ToListAsync();
                        // Add to DB
                        _log.LogDebug("We have the episodes, adding to db transaction");
                        var episodesToAdd = monitoredEpisodes.Select(episode =>
                                new SonarrEpisodeCache
                                {
                                    EpisodeNumber = episode.episodeNumber,
                                    SeasonNumber = episode.seasonNumber,
                                    TvDbId = s.tvdbId,
                                    HasFile = episode.hasFile
                                });
                        //var episodesToAdd = new List<SonarrEpisodeCache>();

                        //foreach (var monitored in monitoredEpisodes)
                        //{
                        //    var existing = allExistingEpisodes.FirstOrDefault(x => x.SeasonNumber == monitored.seasonNumber && x.EpisodeNumber == monitored.episodeNumber);
                        //    if (existing == null)
                        //    {
                        //        // Just add a new one
                        //        episodesToAdd.Add(new SonarrEpisodeCache
                        //        {
                        //            EpisodeNumber = monitored.episodeNumber,
                        //            SeasonNumber = monitored.seasonNumber,
                        //            TvDbId = s.tvdbId,
                        //            HasFile = monitored.hasFile
                        //        });
                        //    } 
                        //    else
                        //    {
                        //        // Do we need to update the availability?
                        //        if (monitored.hasFile != existing.HasFile)
                        //        {
                        //            existing.HasFile = monitored.hasFile;
                        //        }
                        //    }

                        //}
                        strat = _ctx.Database.CreateExecutionStrategy();
                        await strat.ExecuteAsync(async () =>
                        {
                            using (var tran = await _ctx.Database.BeginTransactionAsync())
                            {
                                await _ctx.SonarrEpisodeCache.AddRangeAsync(episodesToAdd);
                                _log.LogDebug("Commiting the transaction");
                                await _ctx.SaveChangesAsync();
                                tran.Commit();
                            }
                        });
                    }

                }

                await OmbiQuartz.TriggerJob(nameof(IArrAvailabilityChecker), "DVR");
            }
            catch (Exception e)
            {
                _log.LogError(LoggingEvents.SonarrCacher, e, "Exception when trying to cache Sonarr");
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
                _ctx?.Dispose();
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