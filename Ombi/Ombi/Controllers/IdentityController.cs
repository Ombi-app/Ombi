using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;
using Ombi.Core.Claims;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Models;
using Ombi.Models;

namespace Ombi.Controllers
{
    [PowerUser]
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


        /// <summary>
        /// This is what the Wizard will call when creating the user for the very first time.
        /// This should never be called after this.
        /// The reason why we return false if users exists is that this method doesn't have any 
        /// authorization and could be called from anywhere.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("Wizard")]
        [AllowAnonymous]
        public async Task<bool> CreateWizardUser([FromBody] UserAuthModel user)
        {
            var users = await IdentityManager.GetUsers();
            if (users.Any())
            {
                // No one should be calling this. Only the wizard
                return false;
            }

            await IdentityManager.CreateUser(new UserDto
            {
                Username = user.Username,
                UserType = UserType.LocalUser,
                Claims = new List<Claim>() {new Claim(ClaimTypes.Role, OmbiClaims.Admin)},
                Password = user.Password,
            });

            return true;
        }
        
    }
}
