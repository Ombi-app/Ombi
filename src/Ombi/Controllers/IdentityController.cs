using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;
using Ombi.Core.Claims;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Models;
using Ombi.Core.Models.UI;
using Ombi.Models;

namespace Ombi.Controllers
{
    /// <summary>
    /// The Identity Controller, the API for everything Identity/User related
    /// </summary>
    /// <seealso cref="Ombi.Controllers.BaseV1ApiController" />
    [PowerUser]
    public class IdentityController : BaseV1ApiController
    {
        public IdentityController(IUserIdentityManager identity, IMapper mapper)
        {
            IdentityManager = identity;
            Mapper = mapper;
        }

        private IUserIdentityManager IdentityManager { get; }
        private IMapper Mapper { get; }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns>Information about the current user</returns>
        [HttpGet]
        public async Task<UserViewModel> GetUser()
        {
            return Mapper.Map<UserViewModel>(await IdentityManager.GetUser(this.HttpContext.User.Identity.Name));
        }


        /// <summary>
        /// This is what the Wizard will call when creating the user for the very first time.
        /// This should never be called after this.
        /// The reason why we return false if users exists is that this method doesn't have any 
        /// authorization and could be called from anywhere.
        /// </summary>
        /// <remarks>We have [AllowAnonymous] since when going through the wizard we do not have a JWT Token yet</remarks>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("Wizard")]
        [ApiExplorerSettings(IgnoreApi = true)]
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
                Claims = new List<Claim>() { new Claim(ClaimTypes.Role, OmbiClaims.Admin) },
                Password = user.Password,
            });

            return true;
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>Information about all users</returns>
        [HttpGet("Users")]
        public async Task<IEnumerable<UserViewModel>> GetAllUsers()
        {
            var type = typeof(OmbiClaims);
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public |
                                                    BindingFlags.Static | BindingFlags.FlattenHierarchy);

            var fields = fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
            var allClaims = fields.Select(x => x.Name).ToList();
            var users = Mapper.Map<IEnumerable<UserViewModel>>(await IdentityManager.GetUsers()).ToList();

            foreach (var user in users)
            {
                var userClaims = user.Claims.Select(x => x.Value);
                var left = allClaims.Except(userClaims);

                foreach (var c in left)
                {
                    user.Claims.Add(new ClaimCheckboxes
                    {
                        Enabled = false,
                        Value = c
                    });
                }
            }

            return users;
        }

        /// <summary>
        /// Gets the user by the user id.
        /// </summary>
        /// <returns>Information about the user</returns>
        [HttpGet("Users/{id}")]
        public async Task<UserViewModel> GetUser(int id)
        {
            var type = typeof(OmbiClaims);
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public |
                                                    BindingFlags.Static | BindingFlags.FlattenHierarchy);

            var fields = fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
            var allClaims = fields.Select(x => x.Name).ToList();
            var user = Mapper.Map<UserViewModel>(await IdentityManager.GetUser(id)).ToList();


            var userClaims = user.Claims.Select(x => x.Value);
            IEnumerable<string> left = allClaims.Except(userClaims);

            foreach (var c in left)
            {
                user.Claims.Add(new ClaimCheckboxes
                {
                    Enabled = false,
                    Value = c
                });
            }


            return user;
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<UserViewModel> CreateUser([FromBody] UserViewModel user)
        {
            user.Id = null;
            var userResult = await IdentityManager.CreateUser(Mapper.Map<UserDto>(user));
            return Mapper.Map<UserViewModel>(userResult);
        }

        /// <summary>
        /// Updates the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<UserViewModel> UpdateUser([FromBody] UserViewModel user)
        {
            var userResult = await IdentityManager.UpdateUser(Mapper.Map<UserDto>(user));
            return Mapper.Map<UserViewModel>(userResult);
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<StatusCodeResult> DeleteUser([FromBody] UserViewModel user)
        {
            await IdentityManager.DeleteUser(Mapper.Map<UserDto>(user));
            return Ok();
        }

        /// <summary>
        /// Gets all available claims in the system.
        /// </summary>
        /// <returns></returns>
        [HttpGet("claims")]
        public IEnumerable<ClaimCheckboxes> GetAllClaims()
        {
            var type = typeof(OmbiClaims);
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public |
                                                    BindingFlags.Static | BindingFlags.FlattenHierarchy);

            var fields = fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
            var allClaims = fields.Select(x => x.Name).ToList();

            return allClaims.Select(x => new ClaimCheckboxes() { Value = x });
        }

    }
}
