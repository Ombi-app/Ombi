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
using Serilog;

namespace Ombi.Schedule.Jobs.Radarr
{
    public class RadarrSync : IRadarrSync
    {
        public RadarrSync(ISettingsService<RadarrSettings> radarr, IRadarrApi radarrApi, ILogger<RadarrSync> log, IOmbiContext ctx)
        {
            RadarrSettings = radarr;
            RadarrApi = radarrApi;
            Logger = log;
            _ctx = ctx;
        }

        private ISettingsService<RadarrSettings> RadarrSettings { get; }
        private IRadarrApi RadarrApi { get; }
        private ILogger<RadarrSync> Logger { get; }
        private readonly IOmbiContext _ctx;

        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        public async Task CacheContent()
        {
            await SemaphoreSlim.WaitAsync();
            try
            {
                var settings = RadarrSettings.GetSettings();
                if (settings.Enabled)
                {
                    try
                    {
                        var movies = await RadarrApi.GetMovies(settings.ApiKey, settings.FullUri);
                        if (movies != null)
                        {
                            // Let's remove the old cached data
                            await _ctx.Database.ExecuteSqlCommandAsync("DELETE FROM RadarrCache");

                            var movieIds = new List<RadarrCache>();
                            foreach (var m in movies)
                            {
                                if (m.tmdbId > 0)
                                {
                                    movieIds.Add(new RadarrCache {TheMovieDbId = m.tmdbId});
                                }
                                else
                                {
                                   Logger.LogError("TMDBId is not > 0 for movie {0}", m.title);
                                }
                            }
                            await _ctx.RadarrCache.AddRangeAsync(movieIds);

                            await _ctx.SaveChangesAsync();
                        }
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
    }
}