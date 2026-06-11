using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ombi.Helpers;

namespace Ombi.Api.IntegrationTests.Harness
{
    /// <summary>
    /// A deterministic authentication handler used by the integration tests. It authenticates
    /// every request as a fixed admin/power user, so we can exercise the [Authorize] endpoints
    /// the mobile app consumes without minting JWTs or going through the real login flow.
    /// </summary>
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string Scheme = "Test";
        public const string TestUserName = "testuser";
        public const string TestUserId = "test-user-id";

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, TestUserName),
                new Claim(ClaimTypes.NameIdentifier, TestUserId),
                new Claim("id", TestUserId),
                new Claim(ClaimTypes.Role, OmbiRoles.Admin),
                new Claim(ClaimTypes.Role, OmbiRoles.PowerUser),
                new Claim(ClaimTypes.Role, OmbiRoles.RequestMovie),
                new Claim(ClaimTypes.Role, OmbiRoles.RequestTv),
            };

            var identity = new ClaimsIdentity(claims, Scheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
