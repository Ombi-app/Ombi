#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: AvailabilityUpdateService.cs
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
using System.Reactive.Linq;
using System.Web.Hosting;

using FluentScheduler;

using Mono.Data.Sqlite;

using NLog;

using PlexRequests.Api;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services.Interfaces;
using PlexRequests.Store;
using PlexRequests.Store.Repository;
using System.Threading.Tasks;

namespace PlexRequests.Services
{
    public class MediaCacheService : ITask, IRegisteredObject, IAvailabilityUpdateService
    {
        public MediaCacheService()
        {
            var memCache = new MemoryCacheProvider();
            var dbConfig = new DbConfiguration(new SqliteFactory());
            var repo = new SettingsJsonRepository(dbConfig, memCache);

            ConfigurationReader = new ConfigurationReader();
            CpCacher = new CouchPotatoCacher(new SettingsServiceV2<CouchPotatoSettings>(repo), new CouchPotatoApi(), memCache);
            SonarrCacher = new SonarrCacher(new SettingsServiceV2<SonarrSettings>(repo), new SonarrApi(), memCache);
            SickRageCacher = new SickRageCacher(new SettingsServiceV2<SickRageSettings>(repo), new SickrageApi(), memCache);
            HostingEnvironment.RegisterObject(this);
        }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        private IConfigurationReader ConfigurationReader { get; }
        private ICouchPotatoCacher CpCacher { get; }
        private ISonarrCacher SonarrCacher { get; }
        private ISickRageCacher SickRageCacher { get; }
        private IDisposable CpSubscription { get; set; }
        private IDisposable SonarrSubscription { get; set; }
        private IDisposable SickRageSubscription { get; set; }

        public void Start(Configuration c)
        {
            CpSubscription?.Dispose();
            SonarrSubscription?.Dispose();
            SickRageSubscription?.Dispose();

            Task.Factory.StartNew(() => CpCacher.Queued(-1));
            Task.Factory.StartNew(() => SonarrCacher.Queued(-1));
            Task.Factory.StartNew(() => SickRageCacher.Queued(-1));
            CpSubscription = Observable.Interval(c.Intervals.Notification).Subscribe(CpCacher.Queued);
            SonarrSubscription = Observable.Interval(c.Intervals.Notification).Subscribe(SonarrCacher.Queued);
            SickRageSubscription = Observable.Interval(c.Intervals.Notification).Subscribe(SickRageCacher.Queued);
        }

        public void Execute()
        {
            Start(ConfigurationReader.Read());
        }

        public void Stop(bool immediate)
        {
            HostingEnvironment.UnregisterObject(this);
        }
    }

    public interface ICouchPotatoCacheService
    {
        void Start(Configuration c);
    }
}
