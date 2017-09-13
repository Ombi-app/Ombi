using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Ombi.Attributes;
using Ombi.Config;
using Ombi.Core.Claims;
using Ombi.Core.Helpers;
using Ombi.Core.Models.UI;
using Ombi.Core.Settings;
using Ombi.Models;
using Ombi.Models.Identity;
using Ombi.Notifications;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using OmbiIdentityResult = Ombi.Models.Identity.IdentityResult;

namespace Ombi.Controllers
{
    /// <summary>
    /// The Identity Controller, the API for everything Identity/User related
    /// </summary>
    /// <seealso cref="Ombi.Controllers.BaseV1ApiController" />
    [PowerUser]
    [ApiV1]
    [Produces("application/json")]
    public class IdentityController : Controller
    {
        public IdentityController(UserManager<OmbiUser> user, IMapper mapper, RoleManager<IdentityRole> rm, IEmailProvider prov,
            ISettingsService<EmailNotificationSettings> s,
            ISettingsService<CustomizationSettings> c,
            IOptions<UserSettings> userSettings)
        {
            UserManager = user;
            Mapper = mapper;
            RoleManager = rm;
            EmailProvider = prov;
            EmailSettings = s;
            CustomizationSettings = c;
            UserSettings = userSettings;
        }

        private UserManager<OmbiUser> UserManager { get; }
        private RoleManager<IdentityRole> RoleManager { get; }
        private IMapper Mapper { get; }
        private IEmailProvider EmailProvider { get; }
        private ISettingsService<EmailNotificationSettings> EmailSettings { get; }
        private ISettingsService<CustomizationSettings> CustomizationSettings { get; }
        private IOptions<UserSettings> UserSettings { get; }

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
                await CreateRoles();
                await UserManager.AddToRoleAsync(userToCreate, OmbiRoles.Admin);
            }

            return true;
        }

        private async Task CreateRoles()
        {
            await CreateRole(OmbiRoles.AutoApproveMovie);
            await CreateRole(OmbiRoles.Admin);
            await CreateRole(OmbiRoles.AutoApproveTv);
            await CreateRole(OmbiRoles.PowerUser);
            await CreateRole(OmbiRoles.RequestMovie);
            await CreateRole(OmbiRoles.RequestTv);
            await CreateRole(OmbiRoles.Disabled);
        }

        private async Task CreateRole(string role)
        {
            if (!await RoleManager.RoleExistsAsync(role))
            {
                await RoleManager.CreateAsync(new IdentityRole(role));
            }
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
            var user = await UserManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);

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
        public async Task<OmbiIdentityResult> CreateUser([FromBody] UserViewModel user)
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
                return new OmbiIdentityResult
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

                return new OmbiIdentityResult
                {
                    Errors = messages
                };
            }

            return new OmbiIdentityResult
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
        public async Task<OmbiIdentityResult> UpdateLocalUser([FromBody] UpdateLocalUserModel ui)
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
                return new OmbiIdentityResult
                {
                    Errors = updateResult.Errors.Select(x => x.Description).ToList()
                };
            }

            if (changingPass)
            {
                var result = await UserManager.ChangePasswordAsync(user, ui.CurrentPassword, ui.Password);

                if (!result.Succeeded)
                {
                    return new OmbiIdentityResult
                    {
                        Errors = result.Errors.Select(x => x.Description).ToList()
                    };
                }
            }
            return new OmbiIdentityResult
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
        public async Task<OmbiIdentityResult> UpdateUser([FromBody] UserViewModel ui)
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
                return new OmbiIdentityResult
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

                return new OmbiIdentityResult
                {
                    Errors = messages
                };
            }

            return new OmbiIdentityResult
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
        public async Task<OmbiIdentityResult> DeleteUser(string userId)
        {

            var userToDelete = await UserManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (userToDelete != null)
            {
                var result = await UserManager.DeleteAsync(userToDelete);
                if (result.Succeeded)
                {
                    return new OmbiIdentityResult
                    {
                        Successful = true
                    };
                }

                return new OmbiIdentityResult
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

        /// <summary>
        /// Send out the email with the reset link
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost("reset")]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<OmbiIdentityResult> SubmitResetPassword([FromBody]SubmitPasswordReset email)
        {
            // Check if account exists
            var user = await UserManager.FindByEmailAsync(email.Email);
            
            var defaultMessage = new OmbiIdentityResult
            {
                Successful = true,
                Errors = new List<string> { "If this account exists you should recieve a password reset link." }
            };
            
            if (user == null)
            {
                return defaultMessage;
            }
            
            // We have the user
            var token = await UserManager.GeneratePasswordResetTokenAsync(user);
            // We now need to email the user with this token
            var emailSettings = await EmailSettings.GetSettingsAsync();
            var customizationSettings = await CustomizationSettings.GetSettingsAsync();
            var appName = (string.IsNullOrEmpty(customizationSettings.ApplicationName)
                ? "Ombi"
                : customizationSettings.ApplicationName);
            
            
            var url =
                $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            await EmailProvider.SendAdHoc(new NotificationMessage
            {
                To = user.Email,
                Subject = $"{appName} Password Reset",
                Message = $"You recently made a request to reset your {appName} account. Please click the link below to complete the process.<br/><br/>" +
                          $"<a href=\"{url}/token?token={token}\"> Reset </a>"
            }, emailSettings);

            return defaultMessage;
        }

        /// <summary>
        /// Resets the password
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("resetpassword")]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<OmbiIdentityResult> ResetPassword([FromBody]ResetPasswordToken token)
        {
            var user = await UserManager.FindByEmailAsync(token.Email);

            if (user == null)
            {
                return new OmbiIdentityResult
                {
                    Successful = false,
                    Errors = new List<string> { "Please check you email." }
                };
            }
            var validToken = WebUtility.UrlDecode(token.Token);
            validToken = validToken.Replace(" ", "+");
            var tokenValid = await UserManager.ResetPasswordAsync(user, validToken, token.Password);

            if (tokenValid.Succeeded)
            {
                return new OmbiIdentityResult
                {
                    Successful = true,
                };
            }
            return new OmbiIdentityResult
            {
                Errors = tokenValid.Errors.Select(x => x.Description).ToList()
            };
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

        private OmbiIdentityResult Error(string message)
        {
            return new OmbiIdentityResult
            {
                Errors = new List<string> { message }
            };
        }
    }
}
