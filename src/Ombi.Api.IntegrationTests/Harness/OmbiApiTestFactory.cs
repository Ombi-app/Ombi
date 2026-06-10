using System;
using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Ombi.Api.External.ExternalApis.RottenTomatoes;
using Ombi.Core;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Engine.V2;
using Ombi.Helpers;

namespace Ombi.Api.IntegrationTests.Harness
{
    /// <summary>
    /// Boots the Ombi API in-memory for contract testing. Search/request engines that would
    /// otherwise reach out to TheMovieDb etc. are swapped for mocks so the tests assert the
    /// serialized HTTP shape (the contract the mobile app depends on) deterministically.
    /// </summary>
    public class OmbiApiTestFactory : WebApplicationFactory<Startup>
    {
        private readonly string _storagePath;

        public Mock<IMovieEngineV2> MovieEngineV2 { get; } = new Mock<IMovieEngineV2>();
        public Mock<ITVSearchEngineV2> TvSearchEngineV2 { get; } = new Mock<ITVSearchEngineV2>();
        public Mock<IMultiSearchEngine> MultiSearchEngine { get; } = new Mock<IMultiSearchEngine>();
        public Mock<IMovieRequestEngine> MovieRequestEngine { get; } = new Mock<IMovieRequestEngine>();
        public Mock<ITvRequestEngine> TvRequestEngine { get; } = new Mock<ITvRequestEngine>();
        public Mock<IRottenTomatoesApi> RottenTomatoesApi { get; } = new Mock<IRottenTomatoesApi>();
        public Mock<IRecentlyAddedEngine> RecentlyAddedEngine { get; } = new Mock<IRecentlyAddedEngine>();

        public OmbiApiTestFactory()
        {
            _storagePath = Path.Combine(Path.GetTempPath(), "ombi-itests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_storagePath);

            // These must be set before the host - and therefore the database and JWT auth - is built.
            StartupSingleton.Instance.StoragePath = _storagePath;
            StartupSingleton.Instance.SecurityKey = "ombi-integration-tests-security-key-please-ignore-1234567890";
        }

        // Bypass Ombi's Program.CreateHostBuilder (which wires up the real Startup with Quartz, the
        // SPA and SignalR) and host only the test startup.
        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<TestStartup>();
                    // UseStartup sets ApplicationName to this test assembly; reset it to the Ombi
                    // assembly (after UseStartup) so MVC discovers its controllers - otherwise it
                    // scans this test assembly and every route 404s.
                    webBuilder.UseSetting(WebHostDefaults.ApplicationKey, typeof(Startup).Assembly.GetName().Name);
                });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(AppContext.BaseDirectory);
            builder.UseEnvironment("Production");

            builder.ConfigureTestServices(services =>
            {
                // Force our deterministic auth scheme as the default for authenticate/challenge.
                services.AddAuthentication(TestAuthHandler.Scheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });
                services.PostConfigure<AuthenticationOptions>(o =>
                {
                    o.DefaultScheme = TestAuthHandler.Scheme;
                    o.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                    o.DefaultChallengeScheme = TestAuthHandler.Scheme;
                });

                // Bypass caching so each request reaches the mocked engine.
                services.RemoveAll<IMediaCacheService>();
                services.AddSingleton<IMediaCacheService, PassThroughMediaCacheService>();

                // Replace engines that hit external services with mocks the tests configure.
                services.AddTransient(_ => MovieEngineV2.Object);
                services.AddTransient(_ => TvSearchEngineV2.Object);
                services.AddTransient(_ => MultiSearchEngine.Object);
                services.AddTransient(_ => MovieRequestEngine.Object);
                services.AddTransient(_ => TvRequestEngine.Object);
                services.AddTransient(_ => RottenTomatoesApi.Object);
                services.AddTransient(_ => RecentlyAddedEngine.Object);
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try
            {
                if (Directory.Exists(_storagePath))
                {
                    Directory.Delete(_storagePath, true);
                }
            }
            catch
            {
                // best-effort cleanup of the temp database directory
            }
        }
    }
}
