using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Models;

namespace Ombi.Controllers
{
    [Authorize]
    public class IdentityController : BaseV1ApiController
    {
        public IdentityController(IUserIdentityManager identity)
        {
            IdentityManager = identity;
        }
        
        private IUserIdentityManager IdentityManager { get; }

        [HttpGet]
        public async Task<UserDto> GetUser()
        {
            return await IdentityManager.GetUser(this.HttpContext.User.Identity.Name);
        }
        
    }
}
