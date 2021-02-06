using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Radarr;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Quartz;
using Serilog;

namespace Ombi.Schedule.Jobs.Radarr
{
    public class RadarrSync : IRadarrSync
    {
        public RadarrSync(ISettingsService<RadarrSettings> radarr, IRadarrApi radarrApi, ILogger<RadarrSync> log, ExternalContext ctx)
        {
            RadarrSettings = radarr;
            RadarrApi = radarrApi;
            Logger = log;
            _ctx = ctx;
            RadarrSettings.ClearCache();
        }

        private ISettingsService<RadarrSettings> RadarrSettings { get; }
        private IRadarrApi RadarrApi { get; }
        private ILogger<RadarrSync> Logger { get; }
        private readonly ExternalContext _ctx;

        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        public async Task Execute(IJobExecutionContext job)
        {
            await SemaphoreSlim.WaitAsync();
            try
            {
                var settings = await RadarrSettings.GetSettingsAsync();
                if (settings.Enabled)
                {
                    try
                    {
                        var movies = await RadarrApi.GetMovies(settings.ApiKey, settings.FullUri);
                        if (movies != null)
                        {
                            var strat = _ctx.Database.CreateExecutionStrategy();
                            await strat.ExecuteAsync(async () =>
                            {
                                // Let's remove the old cached data
                                using (var tran = await _ctx.Database.BeginTransactionAsync())
                                {
                                    await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM RadarrCache");
                                    tran.Commit();
                                }
                            });

                            var movieIds = new List<RadarrCache>();
                            foreach (var m in movies)
                            {
                                if (m.monitored || m.hasFile)
                                {
                                    if (m.tmdbId > 0)
                                    {
                                        movieIds.Add(new RadarrCache
                                        {
                                            TheMovieDbId = m.tmdbId,
                                            HasFile = m.hasFile
                                        });
                                    }
                                    else
                                    {
                                        Logger.LogError("TMDBId is not > 0 for movie {0}", m.title);
                                    }
                                }
                            }
                            strat = _ctx.Database.CreateExecutionStrategy();
                            await strat.ExecuteAsync(async () =>
                            {
                                using (var tran = await _ctx.Database.BeginTransactionAsync())
                                {
                                    await _ctx.RadarrCache.AddRangeAsync(movieIds);

                                    await _ctx.SaveChangesAsync();
                                    tran.Commit();
                                }
                            });
                        }

                        await OmbiQuartz.TriggerJob(nameof(IArrAvailabilityChecker), "DVR");
                    }
                    catch (System.Exception ex)
                    {
                        Logger.LogError(LoggingEvents.Cacher, ex, "Failed caching queued items from Radarr");
                    }
                }
            }
            catch (Exception)
            {
                Logger.LogInformation(LoggingEvents.RadarrCacher, "Radarr is not setup, cannot cache episodes");
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        public async Task<IEnumerable<RadarrCache>> GetCachedContent()
        {
            return await _ctx.RadarrCache.ToListAsync();
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _ctx?.Dispose();
                //RadarrSettings?.Dispose();
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
