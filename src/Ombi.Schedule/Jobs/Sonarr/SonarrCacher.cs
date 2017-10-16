using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Sonarr;
using Ombi.Api.Sonarr.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Schedule.Jobs.Sonarr
{
    public class SonarrCacher : ISonarrCacher
    {
        public SonarrCacher(ISettingsService<SonarrSettings> s, ISonarrApi api, ILogger<SonarrCacher> l, IOmbiContext ctx)
        {
            _settings = s;
            _api = api;
            _log = l;
            _ctx = ctx;
        }

        private readonly ISettingsService<SonarrSettings> _settings;
        private readonly ISonarrApi _api;
        private readonly ILogger<SonarrCacher> _log;
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

                    var episodesToAdd = new List<SonarrEpisodeCache>();
                    await _ctx.Database.ExecuteSqlCommandAsync("DELETE FROM SonarrEpisodeCache");
                    foreach (var s in sonarrSeries)
                    {
                        var episodes = await _api.GetEpisodes(s.id, settings.ApiKey, settings.FullUri);
                        var monitoredEpisodes = episodes.Where(x => x.monitored || x.hasFile);
                        episodesToAdd.AddRange(monitoredEpisodes.Select(episode => new SonarrEpisodeCache
                        {
                            EpisodeNumber = episode.episodeNumber,
                            SeasonNumber = episode.seasonNumber,
                            TvDbId = s.tvdbId
                        }));
                    }

                    await _ctx.SonarrEpisodeCache.AddRangeAsync(episodesToAdd);
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


        //public void Queued()
        //{
        //    var settings = SonarrSettings.GetSettings();
        //    if (settings.Enabled)
        //    {
        //        Job.SetRunning(true, JobNames.SonarrCacher);
        //        try
        //        {
        //            var series = SonarrApi.GetSeries(settings.ApiKey, settings.FullUri);
        //            if (series != null)
        //            {
        //                Cache.Set(CacheKeys.SonarrQueued, series, CacheKeys.TimeFrameMinutes.SchedulerCaching);
        //            }
        //        }
        //        catch (System.Exception ex)
        //        {
        //            Log.Error(ex, "Failed caching queued items from Sonarr");
        //        }
        //        finally
        //        {
        //            Job.Record(JobNames.SonarrCacher);
        //            Job.SetRunning(false, JobNames.SonarrCacher);
        //        }
        //    }
        //}

        //// we do not want to set here...
        //public IEnumerable<SonarrCachedResult> QueuedIds()
        //{
        //    var result = new List<SonarrCachedResult>();

        //    var series = Cache.Get<List<Series>>(CacheKeys.SonarrQueued);
        //    if (series != null)
        //    {
        //        foreach (var s in series)
        //        {
        //            var cached = new SonarrCachedResult { TvdbId = s.tvdbId };
        //            foreach (var season in s.seasons)
        //            {
        //                cached.Seasons.Add(new SonarrSeasons
        //                {
        //                    SeasonNumber = season.seasonNumber,
        //                    Monitored = season.monitored
        //                });
        //            }

        //            result.Add(cached);
        //        }
        //    }
        //    return result;
        //}
    }
}