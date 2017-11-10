using Microsoft.AspNetCore.Authorization;
using Ombi.Helpers;

namespace Ombi.Attributes
{
    public class PowerUserAttribute : AuthorizeAttribute
    {
        public PowerUserAttribute()
        {
            var roles = new []
            {
                OmbiRoles.Admin,
                OmbiRoles.PowerUser
            };
            Roles = string.Join(",",roles);
        }
    }
}