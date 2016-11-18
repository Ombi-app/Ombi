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

using System.Diagnostics;
using System.Net;

using Mono.Data.Sqlite;

using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Nancy.Conventions;
using Nancy.Cryptography;
using Nancy.Diagnostics;
using Nancy.Hosting.Self;
using Nancy.Session;

using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Notification;
using PlexRequests.Store;
using PlexRequests.Store.Repository;
using PlexRequests.UI.Helpers;
using Nancy.Json;

using Ninject;
using PlexRequests.UI.Authentication;

namespace PlexRequests.UI
{
    public class Bootstrapper : NinjectNancyBootstrapper
    {
        // The bootstrapper enables you to reconfigure the composition of the framework,
        // by overriding the various methods and properties.
        // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper

        public Bootstrapper(IKernel kernel)
        {
            _kernel = kernel;
        }

        private IKernel _kernel;
        protected override IKernel GetApplicationContainer()
        {
            Debug.WriteLine("GetAppContainer");
            _kernel.Load<FactoryModule>();
            return _kernel;
        }

        protected override void ApplicationStartup(IKernel container, IPipelines pipelines)
        {
            Debug.WriteLine("Bootstrapper.ApplicationStartup");
            ConfigureContainer(container);

            JsonSettings.MaxJsonLength = int.MaxValue;

            CookieBasedSessions.Enable(pipelines, CryptographyConfiguration.Default);
            StaticConfiguration.DisableErrorTraces = false;

            base.ApplicationStartup(container, pipelines);


            var settings = new SettingsServiceV2<PlexRequestSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            var baseUrl = settings.GetSettings().BaseUrl;
            var redirect = string.IsNullOrEmpty(baseUrl) ? "~/login" : $"~/{baseUrl}/login";

            // Enable forms auth
            var config = new CustomAuthenticationConfiguration
            {
                RedirectUrl = redirect,
                PlexUserRepository = container.Get<IPlexUserRepository>(),
                LocalUserRepository = container.Get<IUserRepository>()
            };

            CustomAuthenticationProvider.Enable(pipelines, config);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback +=
                 (sender, certificate, chain, sslPolicyErrors) => true;

            SubscribeAllObservers(container);

        }

#if DEBUG
        /// <summary>
        /// Set's the root path to the views folder, this means we don't have to recompile the views for every change.
        /// </summary>
        protected override IRootPathProvider RootPathProvider => new DebugRootPathProvider();
#endif
#if !DEBUG

        protected override IRootPathProvider RootPathProvider => new FileSystemRootPathProvider();
#endif
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            Debug.WriteLine("Configuring the conventions");
            base.ConfigureConventions(nancyConventions);

            var settingsService = new SettingsServiceV2<PlexRequestSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            var settings = settingsService.GetSettings();
            var assetLocation = string.Empty;
            if (!string.IsNullOrEmpty(settings.BaseUrl))
            {
                assetLocation = $"{settings.BaseUrl}/";
            }

            Debug.WriteLine($"AssetLocation {assetLocation}");

            nancyConventions.StaticContentsConventions.AddDirectory($"{assetLocation}Content", "Content");
            nancyConventions.StaticContentsConventions.AddDirectory($"{assetLocation}docs", "swagger-ui");
            nancyConventions.StaticContentsConventions.AddDirectory($"{assetLocation}fonts", "Content/fonts");
        }

        protected override DiagnosticsConfiguration DiagnosticsConfiguration => new DiagnosticsConfiguration { Password = @"password" };

        private void SubscribeAllObservers(IKernel container)
        {
            var notificationService = container.Get<INotificationService>();

            var emailSettingsService = container.Get<ISettingsService<EmailNotificationSettings>>();
            var emailSettings = emailSettingsService.GetSettings();
            if (emailSettings.Enabled)
            {
                notificationService.Subscribe(new EmailMessageNotification(emailSettingsService));
            }

            var pushbulletService = container.Get<ISettingsService<PushbulletNotificationSettings>>();
            var pushbulletSettings = pushbulletService.GetSettings();
            if (pushbulletSettings.Enabled)
            {
                notificationService.Subscribe(new PushbulletNotification(container.Get<IPushbulletApi>(), pushbulletService));
            }

            var pushoverService = container.Get<ISettingsService<PushoverNotificationSettings>>();
            var pushoverSettings = pushoverService.GetSettings();
            if (pushoverSettings.Enabled)
            {
                notificationService.Subscribe(new PushoverNotification(container.Get<IPushoverApi>(), pushoverService));
            }

            var slackService = container.Get<ISettingsService<SlackNotificationSettings>>();
            var slackSettings = slackService.GetSettings();
            if (slackSettings.Enabled)
            {
                notificationService.Subscribe(new SlackNotification(container.Get<ISlackApi>(), slackService));
            }
        }

        protected override void RequestStartup(IKernel container, IPipelines pipelines, NancyContext context)
        {
            //CORS Enable
            pipelines.AfterRequest.AddItemToEndOfPipeline((ctx) =>
            {
                ctx.Response.WithHeader("Access-Control-Allow-Origin", "*")
                                .WithHeader("Access-Control-Allow-Methods", "POST,GET")
                                .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type");

            });
            base.RequestStartup(container, pipelines, context);
        }

        private void ConfigureContainer(IKernel container)
        {
            Debug.WriteLine("Configuring ServiceLoc/Container");
            var loc = ServiceLocator.Instance;
            loc.SetContainer(container);
        }

    }
}