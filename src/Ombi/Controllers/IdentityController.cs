using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Ombi.Attributes;
using Ombi.Core.Claims;
using Ombi.Core.Helpers;
using Ombi.Core.Models.UI;
using Ombi.Models;
using Ombi.Models.Identity;
using Ombi.Store.Entities;
using IdentityResult = Ombi.Models.Identity.IdentityResult;

namespace Ombi.Controllers
{
    /// <summary>
    /// The Identity Controller, the API for everything Identity/User related
    /// </summary>
    /// <seealso cref="Ombi.Controllers.BaseV1ApiController" />
    [PowerUser]
    public class IdentityController : BaseV1ApiController
    {
        public IdentityController(UserManager<OmbiUser> user, IMapper mapper, RoleManager<IdentityRole> rm)
        {
            UserManager = user;
            Mapper = mapper;
            RoleManager = rm;
        }

        private UserManager<OmbiUser> UserManager { get; }
        private RoleManager<IdentityRole> RoleManager { get; }
        private IMapper Mapper { get; }

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
            var users = UserManager.Users;
            if (users.Any())
            {
                // No one should be calling this. Only the wizard
                return false;
            }

            var userToCreate = new OmbiUser
            {
                UserName = user.Username,
                UserType = UserType.LocalUser
            };

            var result = await UserManager.CreateAsync(userToCreate, user.Password);
            if (result.Succeeded)
            {
                if (!await RoleManager.RoleExistsAsync(OmbiRoles.Admin))
                {
                    await RoleManager.CreateAsync(new IdentityRole(OmbiRoles.Admin));
                }
                await UserManager.AddToRoleAsync(userToCreate, OmbiRoles.Admin);
            }

            return true;
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>Information about all users</returns>
        [HttpGet("Users")]
        public async Task<IEnumerable<UserViewModel>> GetAllUsers()
        {
            var users = await UserManager.Users
                .ToListAsync();

            var model = new List<UserViewModel>();

            foreach (var user in users)
            {
                model.Add(await GetUserWithRoles(user));
            }

            return model;
        }

        /// <summary>
        /// Gets the current logged in user.
        /// </summary>
        /// <returns>Information about all users</returns>
        [HttpGet]
        [Authorize]
        public async Task<UserViewModel> GetCurrentUser()
        {
            var user = await UserManager.GetUserAsync(User);

            return await GetUserWithRoles(user);
        }

        /// <summary>
        /// Gets the user by the user id.
        /// </summary>
        /// <returns>Information about the user</returns>
        [HttpGet("User/{id}")]
        public async Task<UserViewModel> GetUser(string id)
        {
            var user = await UserManager.Users.FirstOrDefaultAsync(x => x.Id == id);

            return await GetUserWithRoles(user);
        }

        private async Task<UserViewModel> GetUserWithRoles(OmbiUser user)
        {
            var userRoles = await UserManager.GetRolesAsync(user);
            var vm = new UserViewModel
            {
                Alias = user.Alias,
                Username = user.UserName,
                Id = user.Id,
                EmailAddress = user.Email,
                UserType = (Core.Models.UserType)(int)user.UserType,
                Claims = new List<ClaimCheckboxes>(),
            };

            foreach (var role in userRoles)
            {
                vm.Claims.Add(new ClaimCheckboxes
                {
                    Value = role,
                    Enabled = true
                });
            }

            // Add the missing claims
            var allRoles = await RoleManager.Roles.ToListAsync();
            var missing = allRoles.Select(x => x.Name).Except(userRoles);
            foreach (var role in missing)
            {
                vm.Claims.Add(new ClaimCheckboxes
                {
                    Value = role,
                    Enabled = false
                });
            }

            return vm;
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name = "user" > The user.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IdentityResult> CreateUser([FromBody] UserViewModel user)
        {
            if (!EmailValidator.IsValidEmail(user.EmailAddress))
            {
                return Error($"The email address {user.EmailAddress} is not a valid format");
            }
            var ombiUser = new OmbiUser
            {
                Alias = user.Alias,
                Email = user.EmailAddress,
                UserName = user.Username,
                UserType = UserType.LocalUser,
            };
            var userResult = await UserManager.CreateAsync(ombiUser, user.Password);

            if (!userResult.Succeeded)
            {
                // We did not create the user
                return new IdentityResult
                {
                    Errors = userResult.Errors.Select(x => x.Description).ToList()
                };
            }

            var roleResult = await AddRoles(user.Claims, ombiUser);

            if (roleResult.Any(x => !x.Succeeded))
            {
                var messages = new List<string>();
                foreach (var errors in roleResult.Where(x => !x.Succeeded))
                {
                    messages.AddRange(errors.Errors.Select(x => x.Description).ToList());
                }

                return new IdentityResult
                {
                    Errors = messages
                };
            }

            return new IdentityResult
            {
                Successful = true
            };
        }

        /// <summary>
        /// This is for the local user to change their details.
        /// </summary>
        /// <param name="ui"></param>
        /// <returns></returns>
        [HttpPut("local")]
        [Authorize]
        public async Task<IdentityResult> UpdateLocalUser([FromBody] UpdateLocalUserModel ui)
        {
            if (string.IsNullOrEmpty(ui.CurrentPassword))
            {
                return Error("You need to provide your current password to make any changes");
            }

            var changingPass = !string.IsNullOrEmpty(ui.Password) || !string.IsNullOrEmpty(ui.ConfirmNewPassword);

            if (changingPass)
            {
                if (!ui.Password.Equals(ui?.ConfirmNewPassword, StringComparison.CurrentCultureIgnoreCase))
                {
                    return Error("Passwords do not match");
                }
            }

            if (!EmailValidator.IsValidEmail(ui.EmailAddress))
            {
                return Error($"The email address {ui.EmailAddress} is not a valid format");
            }
            // Get the user
            var user = await UserManager.Users.FirstOrDefaultAsync(x => x.Id == ui.Id);
            if (user == null)
            {
                return Error("The user does not exist");
            }

            // Make sure the pass is ok
            var passwordCheck = await UserManager.CheckPasswordAsync(user, ui.CurrentPassword);
            if (!passwordCheck)
            {
                return Error("Your password is incorrect");
            }

            user.Email = ui.EmailAddress;

            var updateResult = await UserManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return new IdentityResult
                {
                    Errors = updateResult.Errors.Select(x => x.Description).ToList()
                };
            }

            if (changingPass)
            {
                var result = await UserManager.ChangePasswordAsync(user, ui.CurrentPassword, ui.Password);

                if (!result.Succeeded)
                {
                    return new IdentityResult
                    {
                        Errors = result.Errors.Select(x => x.Description).ToList()
                    };
                }
            }
            return new IdentityResult
            {
                Successful = true
            };

        }

        /// <summary>
        /// Updates the user.
        /// </summary>
        /// <param name = "ui" > The user.</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IdentityResult> UpdateUser([FromBody] UserViewModel ui)
        {
            if (!EmailValidator.IsValidEmail(ui.EmailAddress))
            {
                return Error($"The email address {ui.EmailAddress} is not a valid format");
            }
            // Get the user
            var user = await UserManager.Users.FirstOrDefaultAsync(x => x.Id == ui.Id);
            user.Alias = ui.Alias;
            user.Email = ui.EmailAddress;
            var updateResult = await UserManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return new IdentityResult
                {
                    Errors = updateResult.Errors.Select(x => x.Description).ToList()
                };
            }

            // Get the roles
            var userRoles = await UserManager.GetRolesAsync(user);

            foreach (var role in userRoles)
            {
                await UserManager.RemoveFromRoleAsync(user, role);
            }

            var result = await AddRoles(ui.Claims, user);
            if (result.Any(x => !x.Succeeded))
            {
                var messages = new List<string>();
                foreach (var errors in result.Where(x => !x.Succeeded))
                {
                    messages.AddRange(errors.Errors.Select(x => x.Description).ToList());
                }

                return new IdentityResult
                {
                    Errors = messages
                };
            }

            return new IdentityResult
            {
                Successful = true
            };

        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <returns></returns>
        [HttpDelete("{userId}")]
        public async Task<IdentityResult> DeleteUser(string userId)
        {

            var userToDelete = await UserManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (userToDelete != null)
            {
                var result = await UserManager.DeleteAsync(userToDelete);
                if (result.Succeeded)
                {
                    return new IdentityResult
                    {
                        Successful = true
                    };
                }

                return new IdentityResult
                {
                    Errors = result.Errors.Select(x => x.Description).ToList()
                };
            }
            return Error("Could not find user to delete.");
        }

        /// <summary>
        /// Gets all available claims in the system.
        /// </summary>
        /// <returns></returns>
        [HttpGet("claims")]
        public async Task<IEnumerable<ClaimCheckboxes>> GetAllClaims()
        {
            var claims = new List<ClaimCheckboxes>();
            // Add the missing claims
            var allRoles = await RoleManager.Roles.ToListAsync();
            var missing = allRoles.Select(x => x.Name);
            foreach (var role in missing)
            {
                claims.Add(new ClaimCheckboxes
                {
                    Value = role,
                    Enabled = false
                });
            }
            return claims;
        }

        private async Task<List<Microsoft.AspNetCore.Identity.IdentityResult>> AddRoles(IEnumerable<ClaimCheckboxes> roles, OmbiUser ombiUser)
        {
            var roleResult = new List<Microsoft.AspNetCore.Identity.IdentityResult>();
            foreach (var role in roles)
            {
                if (role.Enabled)
                {
                    roleResult.Add(await UserManager.AddToRoleAsync(ombiUser, role.Value));
                }
            }
            return roleResult;
        }

        private IdentityResult Error(string message)
        {
            return new IdentityResult
            {
                Errors = new List<string> { message }
            };
        }
    }
}
