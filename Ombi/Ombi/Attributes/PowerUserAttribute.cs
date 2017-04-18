using Microsoft.AspNetCore.Authorization;

namespace Ombi.Attributes
{
    public class PowerUserAttribute : AuthorizeAttribute
    {
        public PowerUserAttribute()
        {
            Roles = "Admin, PowerUser";
        }
    }
}