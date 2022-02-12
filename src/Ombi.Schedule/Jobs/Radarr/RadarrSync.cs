using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Radarr;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Schedule.Jobs.Radarr
{
    public class RadarrSync : IRadarrSync
    {
        public RadarrSync(ISettingsService<RadarrSettings> radarr, ISettingsService<Radarr4KSettings> radarr4k, IRadarrV3Api radarrApi, ILogger<RadarrSync> log, ExternalContext ctx,
            IExternalRepository<RadarrCache> radarrRepo)
        {
            _radarrSettings = radarr;
            _radarr4kSettings = radarr4k;
            _api = radarrApi;
            _logger = log;
            _ctx = ctx;
            _radarrRepo = radarrRepo;
            _radarrSettings.ClearCache();
            _radarr4kSettings.ClearCache();
        }

        private readonly ISettingsService<RadarrSettings> _radarrSettings;
        private readonly ISettingsService<Radarr4KSettings> _radarr4kSettings;
        private readonly IRadarrV3Api _api;
        private readonly ILogger<RadarrSync> _logger;
        private readonly ExternalContext _ctx;
        private readonly IExternalRepository<RadarrCache> _radarrRepo;

        public async Task Execute(IJobExecutionContext job)
        {
            try
            {
                var strat = _ctx.Database.CreateExecutionStrategy();
                await strat.ExecuteAsync(async () =>
                {
                    // Let's remove the old cached data
                    using var tran = await _ctx.Database.BeginTransactionAsync();
                    await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM RadarrCache");
                    tran.Commit();
                });

                var radarrSettings = _radarrSettings.GetSettingsAsync();
                var radarr4kSettings = _radarr4kSettings.GetSettingsAsync();
                await Process(await radarrSettings);
                await Process(await radarr4kSettings);
            }
            catch (Exception)
            {
                _logger.LogInformation(LoggingEvents.RadarrCacher, "Radarr is not setup, cannot cache episodes");
            }
        }

        private async Task Process(RadarrSettings settings)
        {
            if (settings.Enabled)
            {
                try
                {
                    var movies = await _api.GetMovies(settings.ApiKey, settings.FullUri);
                    var existingMovies = _radarrRepo.GetAll();
                    if (movies != null)
                    {
                        var movieIds = new List<RadarrCache>();
                        foreach (var m in movies)
                        {
                            if (m.monitored || m.hasFile)
                            {
                                if (m.tmdbId > 0)
                                {
                                    var is4k = m.movieFile?.quality?.quality?.resolution >= 2160;

                                    // Do we have a cached movie for this already?
                                    var existing = await existingMovies.FirstOrDefaultAsync(x => x.TheMovieDbId == m.tmdbId);
                                    if (existing != null)
                                    {
                                        existing.Has4K = is4k;
                                        existing.HasFile = m.hasFile;
                                    }
                                    else
                                    {
                                        movieIds.Add(new RadarrCache
                                        {
                                            TheMovieDbId = m.tmdbId,
                                            HasFile = m.hasFile,
                                            Has4K = is4k,
                                            HasRegular = !is4k
                                        });
                                    }
                                }
                                else
                                {
                                    _logger.LogError($"TMDBId is not > 0 for movie {m.title}");
                                }
                            }
                        }

                        // Save from the updates made to the existing movies (they are in the EF Change Tracker)
                        await _radarrRepo.SaveChangesAsync();

                        await _radarrRepo.AddRange(movieIds);
                    }

                    await OmbiQuartz.TriggerJob(nameof(IArrAvailabilityChecker), "DVR");
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(LoggingEvents.Cacher, ex, "Failed caching queued items from Radarr");
                }
            }
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
