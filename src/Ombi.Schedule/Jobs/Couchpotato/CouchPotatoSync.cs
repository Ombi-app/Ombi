#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: CouchPotatoCacher.cs
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.CouchPotato;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Quartz;

namespace Ombi.Schedule.Jobs.Couchpotato
{
    public class CouchPotatoSync : ICouchPotatoSync
    {
        public CouchPotatoSync(ISettingsService<CouchPotatoSettings> cpSettings,
            ICouchPotatoApi api, ILogger<CouchPotatoSync> log, ExternalContext ctx, IHubContext<NotificationHub> hub)
        {
            _settings = cpSettings;
            _api = api;
            _log = log;
            _ctx = ctx;
            _notification = hub;
            _settings.ClearCache();
        }

        private readonly ISettingsService<CouchPotatoSettings> _settings;
        private readonly ICouchPotatoApi _api;
        private readonly ILogger<CouchPotatoSync> _log;
        private readonly ExternalContext _ctx;
        private readonly IHubContext<NotificationHub> _notification;

        public async Task Execute(IJobExecutionContext job)
        {
            var settings = await _settings.GetSettingsAsync();
            if (!settings.Enabled)
            {
                return;
            }

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Couch Potato Sync Started");
            try
            {
                _log.LogInformation(LoggingEvents.CouchPotatoCacher, "Getting all active movies from CP");
                var movies = await _api.GetMovies(settings.FullUri, settings.ApiKey, new[] {"active"});
                if (movies != null)
                {
                    var strat = _ctx.Database.CreateExecutionStrategy();
                    await strat.ExecuteAsync(async () =>
                    {
                        // Let's remove the old cached data
                        using (var tran = await _ctx.Database.BeginTransactionAsync())
                        {
                            await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM CouchPotatoCache");
                            tran.Commit();
                        }
                    });

                    // Save
                    var movieIds = new List<CouchPotatoCache>();
                    foreach (var m in movies.movies)
                    {
                        if (m.info == null)
                        {
                            _log.LogInformation("Movie {0} does nto have a tmdbid", m.title);
                            continue;
                        }
                        if (m.info.tmdb_id > 0)
                        {
                            movieIds.Add(new CouchPotatoCache { TheMovieDbId = m.info.tmdb_id });
                        }
                        else
                        {
                            _log.LogError("TMDBId is not > 0 for movie {0}", m.title);
                        }
                    }
                    strat = _ctx.Database.CreateExecutionStrategy();
                    await strat.ExecuteAsync(async () =>
                    {
                        using (var tran = await _ctx.Database.BeginTransactionAsync())
                        {
                            await _ctx.CouchPotatoCache.AddRangeAsync(movieIds);

                            await _ctx.SaveChangesAsync();
                            tran.Commit();
                        }
                    });

                    await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                        .SendAsync(NotificationHub.NotificationEvent, "Couch Potato Sync Finished");
                }
            }
            catch (Exception e)
            {
                await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                    .SendAsync(NotificationHub.NotificationEvent, "Couch Potato Sync Failed");
                _log.LogError(LoggingEvents.CouchPotatoCacher, e, "error when trying to get movies from CP");
                throw;
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