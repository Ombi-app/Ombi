using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Entities;

namespace Ombi.Core.IdentityResolver
{
    public class OmbiProfileService  : IProfileService
    {
        public OmbiProfileService(UserManager<OmbiUser> um)
        {
            UserManager = um;
        }
        
        private UserManager<OmbiUser> UserManager { get; }
        
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {

            if (context.RequestedClaimTypes.Any())
            {
                var user = await UserManager.Users.FirstOrDefaultAsync(x => x.UserName == context.Subject.GetSubjectId());
                if (user != null)
                {
                    var roles = await UserManager.GetRolesAsync(user);
                    var claims = new List<Claim>
                    {
                        new Claim(JwtClaimTypes.Name, user.UserName),
                        new Claim(JwtClaimTypes.Email, user.Email)
                    };

                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(JwtClaimTypes.Role, role));
                    }
                    context.AddFilteredClaims(claims);
                    context.IssuedClaims.AddRange(claims);
                }
            }
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.FromResult(0);
        }
    }
}