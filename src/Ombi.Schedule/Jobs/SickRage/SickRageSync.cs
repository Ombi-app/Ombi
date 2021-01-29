using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.SickRage;
using Ombi.Api.SickRage.Models;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Quartz;

namespace Ombi.Schedule.Jobs.SickRage
{
    public class SickRageSync : ISickRageSync
    {
        public SickRageSync(ISettingsService<SickRageSettings> s, ISickRageApi api, ILogger<SickRageSync> l, ExternalContext ctx)
        {
            _settings = s;
            _api = api;
            _log = l;
            _ctx = ctx;
            _settings.ClearCache();
        }

        private readonly ISettingsService<SickRageSettings> _settings;
        private readonly ISickRageApi _api;
        private readonly ILogger<SickRageSync> _log;
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
                
                var shows = await _api.GetShows(settings.ApiKey, settings.FullUri);
                if (shows != null)
                {
                    var srShows = shows.data.Values;
                    var ids = srShows.Select(x => x.tvdbid); 
                    var strat = _ctx.Database.CreateExecutionStrategy();
                    await strat.ExecuteAsync(async () =>
                    {
                        using (var tran = await _ctx.Database.BeginTransactionAsync())
                        {
                            await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM SickRageCache");
                            tran.Commit();
                        }
                    });

                    var entites = ids.Select(id => new SickRageCache { TvDbId = id }).ToList();

                    await _ctx.SickRageCache.AddRangeAsync(entites);

                    var episodesToAdd = new List<SickRageEpisodeCache>();
                    await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM SickRageEpisodeCache");
                    foreach (var s in srShows)
                    {
                        var seasons = await _api.GetSeasonList(s.tvdbid, settings.ApiKey, settings.FullUri);
                        foreach (var season in seasons.data)
                        {
                            var episodes =
                                await _api.GetEpisodesForSeason(s.tvdbid, season, settings.ApiKey, settings.FullUri);

                            var monitoredEpisodes = episodes.data.Where(x => x.Value.status.Equals(SickRageStatus.Wanted));

                            episodesToAdd.AddRange(monitoredEpisodes.Select(episode => new SickRageEpisodeCache
                            {
                                EpisodeNumber = episode.Key,
                                SeasonNumber = season,
                                TvDbId = s.tvdbid
                            }));
                        }

                    }
                    strat = _ctx.Database.CreateExecutionStrategy();
                    await strat.ExecuteAsync(async () =>
                    {
                        using (var tran = await _ctx.Database.BeginTransactionAsync())
                        {
                            await _ctx.SickRageEpisodeCache.AddRangeAsync(episodesToAdd);
                            await _ctx.SaveChangesAsync();
                            tran.Commit();
                        }
                    });
                }
            }
            catch (Exception e)
            {
                _log.LogError(LoggingEvents.SickRageCacher, e, "Exception when trying to cache SickRage");
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