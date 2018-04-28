using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ombi.Api.Plex;
using Ombi.Attributes;
using Ombi.Config;
using Ombi.Core.Authentication;
using Ombi.Core.Helpers;
using Ombi.Core.Models.UI;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Models;
using Ombi.Models.Identity;
using Ombi.Notifications;
using Ombi.Notifications.Models;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using IdentityResult = Microsoft.AspNetCore.Identity.IdentityResult;
using OmbiIdentityResult = Ombi.Models.Identity.IdentityResult;

namespace Ombi.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// The Identity Controller, the API for everything Identity/User related
    /// </summary>
    [ApiV1]
    [Produces("application/json")]
    public class IdentityController : Controller
    {
        public IdentityController(OmbiUserManager user, IMapper mapper, RoleManager<IdentityRole> rm, IEmailProvider prov,
            ISettingsService<EmailNotificationSettings> s,
            ISettingsService<CustomizationSettings> c,
            ISettingsService<OmbiSettings> ombiSettings,
            IWelcomeEmail welcome,
            IMovieRequestRepository m,
            ITvRequestRepository t,
            ILogger<IdentityController> l,
            IPlexApi plexApi,
            ISettingsService<PlexSettings> settings,
            IRepository<RequestLog> requestLog,
            IRepository<Issues> issues,
            IRepository<IssueComments> issueComments)
        {
            UserManager = user;
            Mapper = mapper;
            RoleManager = rm;
            EmailProvider = prov;
            EmailSettings = s;
            CustomizationSettings = c;
            WelcomeEmail = welcome;
            MovieRepo = m;
            TvRepo = t;
            _log = l;
            _plexApi = plexApi;
            _plexSettings = settings;
            _issuesRepository = issues;
            _requestLogRepository = requestLog;
            _issueCommentsRepository = issueComments;
            OmbiSettings = ombiSettings;
        }

        private OmbiUserManager UserManager { get; }
        private RoleManager<IdentityRole> RoleManager { get; }
        private IMapper Mapper { get; }
        private IEmailProvider EmailProvider { get; }
        private ISettingsService<EmailNotificationSettings> EmailSettings { get; }
        private ISettingsService<CustomizationSettings> CustomizationSettings { get; }
        private ISettingsService<OmbiSettings> OmbiSettings { get; }
        private IWelcomeEmail WelcomeEmail { get; }
        private IMovieRequestRepository MovieRepo { get; }
        private ITvRequestRepository TvRepo { get; }
        private readonly ILogger<IdentityController> _log;
        private readonly IPlexApi _plexApi;
        private readonly ISettingsService<PlexSettings> _plexSettings;
        private readonly IRepository<Issues> _issuesRepository;
        private readonly IRepository<IssueComments> _issueCommentsRepository;
        private readonly IRepository<RequestLog> _requestLogRepository;


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
        public async Task<bool> CreateWizardUser([FromBody] CreateUserWizardModel user)
        {
            var users = UserManager.Users;
            if (users.Any(x => !x.UserName.Equals("api", StringComparison.CurrentCultureIgnoreCase)))
            {
                // No one should be calling this. Only the wizard
                return false;
            }

            if (user.UsePlexAdminAccount)
            {
                var settings = await _plexSettings.GetSettingsAsync();
                var authToken = settings.Servers.FirstOrDefault()?.PlexAuthToken ?? string.Empty;
                if (authToken.IsNullOrEmpty())
                {
                    _log.LogWarning("Could not find an auth token to create the plex user with");
                    return false;
                }
                var plexUser = await _plexApi.GetAccount(authToken);
                var adminUser = new OmbiUser
                {
                    UserName = plexUser.user.username,
                    UserType = UserType.PlexUser,
                    Email = plexUser.user.email,
                    ProviderUserId = plexUser.user.id
                };

                return await SaveWizardUser(user, adminUser);
            }

            var userToCreate = new OmbiUser
            {
                UserName = user.Username,
                UserType = UserType.LocalUser
            };

            return await SaveWizardUser(user, userToCreate);
        }

        private async Task<bool> SaveWizardUser(CreateUserWizardModel user, OmbiUser userToCreate)
        {
            IdentityResult result;
            // When creating the admin as the plex user, we do not pass in the password.
            if (user.Password.HasValue())
            {
                result = await UserManager.CreateAsync(userToCreate, user.Password);
            }
            else
            {
                result = await UserManager.CreateAsync(userToCreate);
            }
            if (result.Succeeded)
            {
                _log.LogInformation("Created User {0}", userToCreate.UserName);
                await CreateRoles();
                _log.LogInformation("Created the roles");
                var roleResult = await UserManager.AddToRoleAsync(userToCreate, OmbiRoles.Admin);
                if (!roleResult.Succeeded)
                {
                    LogErrors(roleResult);
                }
                else
                {
                    _log.LogInformation("Added the Admin role");
                }
            }
            if (!result.Succeeded)
            {
                LogErrors(result);
            }

            // Update the wizard flag
            var settings = await OmbiSettings.GetSettingsAsync();
            settings.Wizard = true;
            await OmbiSettings.SaveSettingsAsync(settings);

            return result.Succeeded;
        }

        private void LogErrors(IdentityResult result)
        {
            foreach (var err in result.Errors)
            {
                _log.LogCritical(err.Description);
            }
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
            await CreateRole(OmbiRoles.RecievesNewsletter);
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
        [PowerUser]
        public async Task<IEnumerable<UserViewModel>> GetAllUsers()
        {
            var users = await UserManager.Users.Where(x => x.UserType != UserType.SystemUser)
                .ToListAsync();

            var model = new List<UserViewModel>();

            foreach (var user in users)
            {
                model.Add(await GetUserWithRoles(user));
            }

            return model.OrderBy(x => x.UserName);
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
        [PowerUser]
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
                UserName = user.UserName,
                Id = user.Id,
                EmailAddress = user.Email,
                UserType = (Core.Models.UserType)(int)user.UserType,
                Claims = new List<ClaimCheckboxes>(),
                LastLoggedIn = user.LastLoggedIn,
                HasLoggedIn = user.LastLoggedIn.HasValue,
                EpisodeRequestLimit = user.EpisodeRequestLimit ?? 0,
                MovieRequestLimit = user.MovieRequestLimit ?? 0
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
        [PowerUser]
        public async Task<OmbiIdentityResult> CreateUser([FromBody] UserViewModel user)
        {
            if (!EmailValidator.IsValidEmail(user.EmailAddress))
            {
                return Error($"The email address {user.EmailAddress} is not a valid format");
            }
            if (!CanModifyUser(user.Claims.Select(x => x.Value)))
            {
                return Error("You do not have the correct permissions to create this user");
            }
            var ombiUser = new OmbiUser
            {
                Alias = user.Alias,
                Email = user.EmailAddress,
                UserName = user.UserName,
                UserType = UserType.LocalUser,
                MovieRequestLimit = user.MovieRequestLimit,
                EpisodeRequestLimit = user.EpisodeRequestLimit,
                UserAccessToken = Guid.NewGuid().ToString("N"),
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

        private bool CanModifyUser(IEnumerable<string> roles)
        {
            if (roles.Any(x => x.Equals("admin", StringComparison.CurrentCultureIgnoreCase)))
            {
                // Only Admins can create admins
                if (!User.IsInRole(OmbiRoles.Admin))
                {
                    return false;
                }
            }
            return true;
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
        [PowerUser]
        public async Task<OmbiIdentityResult> UpdateUser([FromBody] UserViewModel ui)
        {
            if (!EmailValidator.IsValidEmail(ui.EmailAddress))
            {
                return Error($"The email address {ui.EmailAddress} is not a valid format");
            }
            if (!CanModifyUser(ui.Claims.Select(x => x.Value)))
            {
                return Error("You do not have the correct permissions to create this user");
            }
            // Get the user
            var user = await UserManager.Users.FirstOrDefaultAsync(x => x.Id == ui.Id);
            user.Alias = ui.Alias;
            user.Email = ui.EmailAddress;
            user.MovieRequestLimit = ui.MovieRequestLimit;
            user.EpisodeRequestLimit = ui.EpisodeRequestLimit;
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

            // Am I modifying myself?
            var modifyingSelf = user.UserName.Equals(User.Identity.Name, StringComparison.CurrentCultureIgnoreCase);

            foreach (var role in userRoles)
            {
                if (modifyingSelf && role.Equals(OmbiRoles.Admin))
                {
                    // We do not want to remove the admin role from yourself, this must be an accident
                    var claim = ui.Claims.FirstOrDefault(x => x.Value == OmbiRoles.Admin && x.Enabled);
                    ui.Claims.Remove(claim);
                    continue;
                }
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
        [PowerUser]
        public async Task<OmbiIdentityResult> DeleteUser(string userId)
        {
            var userToDelete = await UserManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (userToDelete != null)
            {

                // Can we delete this user?
                var userRoles = await UserManager.GetRolesAsync(userToDelete);
                if (!CanModifyUser(userRoles))
                {
                    return Error("You do not have the correct permissions to delete this user");
                }

                // We need to delete all the requests first
                var moviesUserRequested = MovieRepo.GetAll().Where(x => x.RequestedUserId == userId);
                var tvUserRequested = TvRepo.GetChild().Where(x => x.RequestedUserId == userId);
                
                if (moviesUserRequested.Any())
                {
                    await MovieRepo.DeleteRange(moviesUserRequested);
                }
                if (tvUserRequested.Any())
                {
                    await TvRepo.DeleteChildRange(tvUserRequested);
                }

                // Delete any issues and request logs
                var issues = _issuesRepository.GetAll().Where(x => x.UserReportedId == userId);
                var issueComments = _issueCommentsRepository.GetAll().Where(x => x.UserId == userId);
                var requestLog = _requestLogRepository.GetAll().Where(x => x.UserId == userId);
                if (issues.Any())
                {
                    await _issuesRepository.DeleteRange(issues);
                }
                if (requestLog.Any())
                {
                    await _requestLogRepository.DeleteRange(requestLog);
                }
                if (issueComments.Any())
                {
                    await _issueCommentsRepository.DeleteRange(issueComments);
                }

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
        [PowerUser]
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


            var customizationSettings = await CustomizationSettings.GetSettingsAsync();
            var appName = (string.IsNullOrEmpty(customizationSettings.ApplicationName)
                ? "Ombi"
                : customizationSettings.ApplicationName);

            var emailSettings = await EmailSettings.GetSettingsAsync();

            customizationSettings.AddToUrl("/token?token=");
            var url = customizationSettings.ApplicationUrl;

            if (user.UserType == UserType.PlexUser)
            {
                await EmailProvider.SendAdHoc(new NotificationMessage
                {
                    To = user.Email,
                    Subject = $"{appName} Password Reset",
                    Message =
                        $"You recently made a request to reset your {appName} account. Please click the link below to complete the process.<br/><br/>" +
                        $"<a href=\"https://www.plex.tv/sign-in/password-reset/\"> Reset </a>"
                }, emailSettings);
            }
            else if (user.UserType == UserType.EmbyUser && user.IsEmbyConnect)
            {
                await EmailProvider.SendAdHoc(new NotificationMessage
                {
                    To = user.Email,
                    Subject = $"{appName} Password Reset",
                    Message =
                        $"You recently made a request to reset your {appName} account.<br/><br/>" +
                        $"To reset your password you need to go to <a href=\"https://emby.media/community/index.php\">Emby.Media</a> and then click on your Username > Edit Profile > Email and Password"
                }, emailSettings);
            }
            else
            {
                // We have the user
                var token = await UserManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebUtility.UrlEncode(token);

                await EmailProvider.SendAdHoc(new NotificationMessage
                {
                    To = user.Email,
                    Subject = $"{appName} Password Reset",
                    Message =
                        $"You recently made a request to reset your {appName} account. Please click the link below to complete the process.<br/><br/>" +
                        $"<a href=\"{url}{encodedToken}\"> Reset </a>"
                }, emailSettings);
            }

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

        [HttpPost("welcomeEmail")]
        [PowerUser]
        public void SendWelcomeEmail([FromBody] UserViewModel user)
        {
            var ombiUser = new OmbiUser
            {
                Email = user.EmailAddress,
                UserName = user.UserName
            };
            BackgroundJob.Enqueue(() => WelcomeEmail.SendEmail(ombiUser));
        }

        [HttpGet("accesstoken")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<string> GetUserAccessToken()
        {
            var user = await UserManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            if (user == null)
            {
                return Guid.Empty.ToString("N");
            }
            if (user.UserAccessToken.IsNullOrEmpty())
            {
                // Let's create an access token for this user
                user.UserAccessToken = Guid.NewGuid().ToString("N");
                var result = await UserManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    LogErrors(result);
                    return Guid.Empty.ToString("N");
                }
            }
            return user.UserAccessToken;
        }

        private async Task<List<IdentityResult>> AddRoles(IEnumerable<ClaimCheckboxes> roles, OmbiUser ombiUser)
        {
            var roleResult = new List<IdentityResult>();
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
