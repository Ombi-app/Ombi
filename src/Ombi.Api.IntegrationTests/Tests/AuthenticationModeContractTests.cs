using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;

namespace Ombi.Api.IntegrationTests.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class AuthenticationModeContractTests : IntegrationTestBase
    {
        private const string LocalUserName = "plex-only-mode-local-user";
        private const string LocalUserPassword = "PlexOnlyModeTest!123";

        [Test]
        public async Task AuthenticationSettings_RejectDisablingLocalLoginWithoutPlexOAuth()
        {
            using var scope = Factory.Services.CreateScope();
            var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService<AuthenticationSettings>>();
            var original = Clone(await settingsService.GetSettingsAsync());

            try
            {
                var invalid = Clone(original);
                invalid.EnableOAuth = false;
                invalid.DisableLocalAuthentication = true;

                var (status, body) = await PostJsonAsync("/api/v1/settings/authentication", invalid);

                Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(body.Trim(), Is.EqualTo("false"));
            }
            finally
            {
                await settingsService.SaveSettingsAsync(original);
            }
        }

        [Test]
        public async Task LocalCredentialLogin_IsRejectedWhenDisabled_AndWorksWhenEnabled()
        {
            using var scope = Factory.Services.CreateScope();
            var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService<AuthenticationSettings>>();
            var userManager = scope.ServiceProvider.GetRequiredService<OmbiUserManager>();
            var original = Clone(await settingsService.GetSettingsAsync());

            var user = await userManager.FindByNameAsync(LocalUserName);
            if (user == null)
            {
                user = new OmbiUser
                {
                    UserName = LocalUserName,
                    UserType = UserType.LocalUser,
                    Alias = "Plex-only mode test user",
                    Email = "plex-only-mode@example.com",
                    StreamingCountry = "US",
                    Language = "en"
                };

                var created = await userManager.CreateAsync(user, LocalUserPassword);
                Assert.That(created.Succeeded, Is.True, string.Join("; ", created.Errors));
            }

            try
            {
                var enabled = Clone(original);
                enabled.DisableLocalAuthentication = false;
                await settingsService.SaveSettingsAsync(enabled);

                var (enabledStatus, _) = await PostJsonAsync("/api/v1/token", new
                {
                    username = LocalUserName,
                    password = LocalUserPassword,
                    rememberMe = false,
                    usePlexOAuth = false
                });
                Assert.That(enabledStatus, Is.EqualTo(HttpStatusCode.OK),
                    "The test account must be able to use the normal Ombi credential path before it is disabled.");

                var plexOnly = Clone(original);
                plexOnly.EnableOAuth = true;
                plexOnly.DisableLocalAuthentication = true;
                await settingsService.SaveSettingsAsync(plexOnly);

                var (disabledStatus, _) = await PostJsonAsync("/api/v1/token", new
                {
                    username = LocalUserName,
                    password = LocalUserPassword,
                    rememberMe = false,
                    usePlexOAuth = false
                });
                Assert.That(disabledStatus, Is.EqualTo(HttpStatusCode.Unauthorized));

                var (requiresPasswordStatus, requiresPasswordBody) = await PostJsonAsync("/api/v1/token/requirePassword", new
                {
                    username = LocalUserName,
                    password = "",
                    rememberMe = false,
                    usePlexOAuth = false
                });
                Assert.That(requiresPasswordStatus, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(requiresPasswordBody.Trim(), Is.EqualTo("true"));
            }
            finally
            {
                await settingsService.SaveSettingsAsync(original);
            }
        }

        private static AuthenticationSettings Clone(AuthenticationSettings settings)
        {
            return JsonConvert.DeserializeObject<AuthenticationSettings>(
                JsonConvert.SerializeObject(settings));
        }
    }
}
