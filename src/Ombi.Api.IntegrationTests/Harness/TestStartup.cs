using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ombi.Core.Authentication;
using Ombi.Core.Settings.Models.External;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Serilog;

namespace Ombi.Api.IntegrationTests.Harness
{
    /// <summary>
    /// A trimmed-down startup for integration tests. It reuses the production
    /// <see cref="Startup.ConfigureServices"/> (full DI graph, EF Core SQLite, MVC + Newtonsoft,
    /// auth) by composition, but replaces the HTTP pipeline so that the things which make in-memory
    /// hosting hard - Quartz scheduling, the Angular SPA static files and the SignalR hub - are
    /// never started.
    /// </summary>
    public class TestStartup
    {
        private readonly Startup _inner;

        public TestStartup(IWebHostEnvironment env)
        {
            _inner = new Startup(env);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            _inner.ConfigureServices(services);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            loggerFactory.AddSerilog();

            // Concrete SQLite contexts run their migrations in their constructor, so resolving them
            // here gives us a fully migrated, empty database. Seed the baseline data the app expects.
            var ctx = serviceProvider.GetRequiredService<OmbiContext>();
            ctx.Seed();
            var settingsCtx = serviceProvider.GetRequiredService<SettingsContext>();
            settingsCtx.Seed();

            SeedPlexSettings(settingsCtx);
            SeedTestUser(serviceProvider).GetAwaiter().GetResult();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        // Pre-populate Plex settings with an InstallId so the read-only client-id endpoint never has
        // to write (a write would try to schedule a Quartz job, which is not running in tests).
        private static void SeedPlexSettings(SettingsContext settingsCtx)
        {
            if (settingsCtx.Settings.Any(x => x.SettingsName == nameof(PlexSettings)))
            {
                return;
            }

            settingsCtx.Settings.Add(new GlobalSettings
            {
                SettingsName = nameof(PlexSettings),
                Content = JsonConvert.SerializeObject(new PlexSettings { Enable = false, InstallId = Guid.NewGuid() })
            });
            settingsCtx.SaveChanges();
        }

        private static async Task SeedTestUser(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<OmbiUserManager>();
            var existing = await userManager.FindByNameAsync(TestAuthHandler.TestUserName);
            if (existing != null)
            {
                return;
            }

            var user = new OmbiUser
            {
                Id = TestAuthHandler.TestUserId,
                UserName = TestAuthHandler.TestUserName,
                UserType = UserType.LocalUser,
                Alias = "Test User",
                Email = "test@example.com",
                StreamingCountry = "US",
                Language = "en"
            };

            await userManager.CreateAsync(user);
        }
    }
}
