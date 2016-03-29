#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: Bootstrapper.cs
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

using System.Net;
using FluentScheduler;
using Mono.Data.Sqlite;

using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Cryptography;
using Nancy.Diagnostics;
using Nancy.Session;
using Nancy.TinyIoc;

using PlexRequests.Api;
using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Mocks;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Notification;
using PlexRequests.Store;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;
using PlexRequests.UI.Jobs;
using TaskFactory = FluentScheduler.TaskFactory;

namespace PlexRequests.UI
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        // The bootstrapper enables you to reconfigure the composition of the framework,
        // by overriding the various methods and properties.
        // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            container.Register<IUserMapper, UserMapper>();
            container.Register<ISqliteConfiguration, DbConfiguration>(new DbConfiguration(new SqliteFactory()));
            container.Register<ICacheProvider, MemoryCacheProvider>();

            // Settings
            container.Register<ISettingsService<PlexRequestSettings>, SettingsServiceV2<PlexRequestSettings>>();
            container.Register<ISettingsService<CouchPotatoSettings>, SettingsServiceV2<CouchPotatoSettings>>();
            container.Register<ISettingsService<AuthenticationSettings>, SettingsServiceV2<AuthenticationSettings>>();
            container.Register<ISettingsService<PlexSettings>, SettingsServiceV2<PlexSettings>>();
            container.Register<ISettingsService<SonarrSettings>, SettingsServiceV2<SonarrSettings>>();
            container.Register<ISettingsService<SickRageSettings>, SettingsServiceV2<SickRageSettings>>();
            container.Register<ISettingsService<EmailNotificationSettings>, SettingsServiceV2<EmailNotificationSettings>>();
            container.Register<ISettingsService<PushbulletNotificationSettings>, SettingsServiceV2<PushbulletNotificationSettings>>();
            container.Register<ISettingsService<PushoverNotificationSettings>, SettingsServiceV2<PushoverNotificationSettings>>();
            container.Register<ISettingsService<HeadphonesSettings>, SettingsServiceV2<HeadphonesSettings>>();

            // Repo's
            container.Register<IRepository<RequestedModel>, GenericRepository<RequestedModel>>();
            container.Register<IRepository<LogEntity>, GenericRepository<LogEntity>>();
            container.Register<IRequestService, JsonRequestService>();
            container.Register<ISettingsRepository, SettingsJsonRepository>();

            // Services
            container.Register<IAvailabilityChecker, PlexAvailabilityChecker>();
            container.Register<IConfigurationReader, ConfigurationReader>();
            container.Register<IIntervals, UpdateInterval>();

            // Api's
            container.Register<ICouchPotatoApi, CouchPotatoApi>();
            container.Register<IPushbulletApi, PushbulletApi>();
            container.Register<IPushoverApi, PushoverApi>();
            container.Register<ISickRageApi, SickrageApi>();
            container.Register<ISonarrApi, SonarrApi>();
            container.Register<IPlexApi, PlexApi>();

            // NotificationService
            container.Register<INotificationService, NotificationService>().AsSingleton();

            SubscribeAllObservers(container);
            base.ConfigureRequestContainer(container, context);
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            TaskManager.TaskFactory = new PlexTaskFactory();
            TaskManager.Initialize(new PlexRegistry());

            CookieBasedSessions.Enable(pipelines, CryptographyConfiguration.Default);

            StaticConfiguration.DisableErrorTraces = false;

            base.ApplicationStartup(container, pipelines);

            // Enable forms auth
            var formsAuthConfiguration = new FormsAuthenticationConfiguration
            {
                RedirectUrl = "~/login",
                UserMapper = container.Resolve<IUserMapper>()
            };

            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);

            ServicePointManager.ServerCertificateValidationCallback +=
                 (sender, certificate, chain, sslPolicyErrors) => true;

        }


        protected override DiagnosticsConfiguration DiagnosticsConfiguration => new DiagnosticsConfiguration { Password = @"password" };

        private void SubscribeAllObservers(TinyIoCContainer container)
        {
            var notificationService = container.Resolve<INotificationService>();

            var emailSettingsService = container.Resolve<ISettingsService<EmailNotificationSettings>>();
            var emailSettings = emailSettingsService.GetSettings();
            if (emailSettings.Enabled)
            {
                notificationService.Subscribe(new EmailMessageNotification(emailSettingsService));
            }

            var pushbulletService = container.Resolve<ISettingsService<PushbulletNotificationSettings>>();
            var pushbulletSettings = pushbulletService.GetSettings();
            if (pushbulletSettings.Enabled)
            {
                notificationService.Subscribe(new PushbulletNotification(container.Resolve<IPushbulletApi>(), pushbulletService));
            }

            var pushoverService = container.Resolve<ISettingsService<PushoverNotificationSettings>>();
            var pushoverSettings = pushoverService.GetSettings();
            if (pushoverSettings.Enabled)
            {
                notificationService.Subscribe(new PushoverNotification(container.Resolve<IPushoverApi>(), pushoverService));
            }
        }
    }
}