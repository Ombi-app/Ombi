using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Ombi.Api.Radarr;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs
{
    public class RadarrCacher : IRadarrCacher
    {
        public RadarrCacher(ISettingsService<RadarrSettings> radarr, IRadarrApi api, IMemoryCache cache, ILogger<RadarrCacher> logger)
        {
            RadarrSettings = radarr;
            Api = api;
            Cache = cache;
            Log = logger;
        }

        private ISettingsService<RadarrSettings> RadarrSettings { get; }
        private IRadarrApi Api { get; }
        private IMemoryCache Cache { get; }
        private ILogger<RadarrCacher> Log { get; }

        public async Task Start()
        {
            var settings = RadarrSettings.GetSettings();
            if (settings.Enabled)
            {
                try
                {
                    var movies = await Api.GetMovies(settings.ApiKey, settings.FullUri);
                    if (movies != null)
                    {
                        var movieIds = new List<int>();
                        foreach (var m in movies)
                        {
                            if (m.tmdbId > 0)
                            {
                                movieIds.Add(m.tmdbId);
                            }
                            else
                            {
                                Log.LogError("TMDBId is not > 0 for movie {0}", m.title);
                            }
                        }
                        Cache.Set<IEnumerable<int>>(CacheKeys.RadarrCacher, movieIds);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.LogError(LoggingEvents.RadarrCacherException, ex, "Failed caching queued items from Radarr");
                }
                finally
                {
                    // Record job run
                }
            }
        }
    }
}
