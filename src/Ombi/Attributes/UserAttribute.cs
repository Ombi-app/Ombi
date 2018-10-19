using Microsoft.AspNetCore.Authorization;
using Ombi.Helpers;


namespace Ombi.Attributes
{
    public class UserAttribute : AuthorizeAttribute
    {
        public UserAttribute()
        {
            Roles = "ManageOwnRequests";
        }
    }
}
