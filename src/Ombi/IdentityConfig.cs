using System.Collections.Generic;
using IdentityModel;
using IdentityServer4.Models;

namespace Ombi
{
    public class IdentityConfig
    {
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource {
                    Name = "role",
                    UserClaims = new List<string> {"role"}
                }
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "API")
                {
                    UserClaims = {JwtClaimTypes.Name, JwtClaimTypes.Role, JwtClaimTypes.Email, JwtClaimTypes.Id},

                }
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients()
        {
            // client credentials client
            return new List<Client>
            {
                new Client
                {
                    ClientId = "frontend",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256()) // TODO read up on what this actually is
                    },
                    AllowedScopes =
                    {
                        "api",
                    },
                    AccessTokenType = AccessTokenType.Jwt
                }
            };
        }
    }
}