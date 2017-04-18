using Microsoft.AspNetCore.Authorization;

namespace Ombi.Attributes
{
    public class AdminAttribute : AuthorizeAttribute
    {
        public AdminAttribute()
        {
            base.Roles = "Admin";
        }
        
    }
}