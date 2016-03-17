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

namespace PlexRequests.Services
{
    public class AvailabilityUpdateService : ITask, IRegisteredObject, IAvailabilityUpdateService
    {
        public AvailabilityUpdateService()
        {
            ConfigurationReader = new ConfigurationReader();
            var repo = new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider());
            Checker = new PlexAvailabilityChecker(new SettingsServiceV2<PlexSettings>(repo), new SettingsServiceV2<AuthenticationSettings>(repo), new RequestService(new GenericRepository<RequestedModel>(new DbConfiguration(new SqliteFactory()))), new PlexApi());
            HostingEnvironment.RegisterObject(this);
        }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        private IConfigurationReader ConfigurationReader { get; }
        private IAvailabilityChecker Checker { get; }
        private IDisposable UpdateSubscription { get; set; }

        public void Start(Configuration c)
        {
            UpdateSubscription?.Dispose();

            UpdateSubscription = Observable.Interval(c.Intervals.Notification).Subscribe(Checker.CheckAndUpdateAll);
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

    public interface IAvailabilityUpdateService
    {
        void Start(Configuration c);
    }
}