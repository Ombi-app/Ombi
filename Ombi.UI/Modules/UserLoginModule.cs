#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: UserLoginModule.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using Nancy.Linker;
using NLog;
using Ombi.Api;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Emby;
using Ombi.Api.Models.Plex;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Core.Users;
using Ombi.Helpers;
using Ombi.Helpers.Analytics;
using Ombi.Helpers.Permissions;
using Ombi.Store;
using Ombi.Store.Models;
using Ombi.Store.Models.Emby;
using Ombi.Store.Models.Plex;
using Ombi.Store.Repository;
using Ombi.UI.Authentication;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;


namespace Ombi.UI.Modules
{
    public class UserLoginModule : BaseModule
    {
        public UserLoginModule(ISettingsService<AuthenticationSettings> auth, IPlexApi api, ISettingsService<PlexSettings> plexSettings, ISettingsService<PlexRequestSettings> pr,
            ISettingsService<LandingPageSettings> lp, IAnalytics a, IResourceLinker linker, IRepository<UserLogins> userLogins, IExternalUserRepository<PlexUsers> plexUsers, ICustomUserMapper custom,
             ISecurityExtensions security, ISettingsService<UserManagementSettings> userManagementSettings, IEmbyApi embyApi, ISettingsService<EmbySettings> emby, IExternalUserRepository<EmbyUsers> embyU,
             IUserHelper userHelper)
            : base("userlogin", pr, security)
        {
            AuthService = auth;
            LandingPageSettings = lp;
            Analytics = a;
            PlexApi = api;
            PlexSettings = plexSettings;
            Linker = linker;
            UserLogins = userLogins;
            PlexUserRepository = plexUsers;
            CustomUserMapper = custom;
            UserManagementSettings = userManagementSettings;
            EmbySettings = emby;
            EmbyApi = embyApi;
            EmbyUserRepository = embyU;
            UserHelper = userHelper;

            Post["/", true] = async (x, ct) => await LoginUser();
            Get["/logout"] = x => Logout();

            Get["UserLoginIndex", "/", true] = async (x, ct) =>
            {
                if (Request.Query["landing"] == null)
                {
                    var s = await LandingPageSettings.GetSettingsAsync();
                    if (s.Enabled)
                    {
                        if (s.BeforeLogin) // Before login
                        {
                            if (string.IsNullOrEmpty(Username))
                            {
                                // They are not logged in
                                return
                                    Context.GetRedirect(Linker.BuildRelativeUri(Context, "LandingPageIndex").ToString());
                            }
                            return Context.GetRedirect(Linker.BuildRelativeUri(Context, "SearchIndex").ToString());
                        }

                        // After login
                        if (string.IsNullOrEmpty(Username))
                        {
                            // Not logged in yet
                            return Context.GetRedirect(Linker.BuildRelativeUri(Context, "UserLoginIndex").ToString() + "?landing");
                        }
                        // Send them to landing
                        var landingUrl = Linker.BuildRelativeUri(Context, "LandingPageIndex").ToString();
                        return Context.GetRedirect(landingUrl);
                    }
                }

                if (!string.IsNullOrEmpty(Username) || IsAdmin)
                {
                    var url = Linker.BuildRelativeUri(Context, "SearchIndex").ToString();
                    return Response.AsRedirect(url);
                }
                var settings = await AuthService.GetSettingsAsync();
                return View["Username", settings];
            };

            Post["/login", true] = async (x, ct) => await UsernameLogin();
            Post["/password", true] = async (x, ct) => await PasswordLogin();
        }

        private ISettingsService<AuthenticationSettings> AuthService { get; }
        private ISettingsService<LandingPageSettings> LandingPageSettings { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
        private IPlexApi PlexApi { get; }
        private IEmbyApi EmbyApi { get; }
        private IResourceLinker Linker { get; }
        private IAnalytics Analytics { get; }
        private IRepository<UserLogins> UserLogins { get; }
        private IExternalUserRepository<PlexUsers> PlexUserRepository { get; }
        private IExternalUserRepository<EmbyUsers> EmbyUserRepository { get; }
        private ICustomUserMapper CustomUserMapper { get; }
        private ISettingsService<UserManagementSettings> UserManagementSettings { get; }
        private IUserHelper UserHelper { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        private async Task<Response> UsernameLogin()
        {
            var username = Request.Form.username.Value;
            var dateTimeOffset = Request.Form.DateTimeOffset;
            var loginGuid = Guid.Empty;
            var settings = await AuthService.GetSettingsAsync();

            if (string.IsNullOrWhiteSpace(username) || IsUserInDeniedList(username, settings))
            {
                return Response.AsJson(new { result = false, message = Resources.UI.UserLogin_IncorrectUserPass });
            }

            var plexSettings = await PlexSettings.GetSettingsAsync();
            var embySettings = await EmbySettings.GetSettingsAsync();

            var authenticated = false;
            var isOwner = false;
            var userId = string.Empty;
            EmbyUser embyUser = null;

            if (plexSettings.Enable)
            {
                if (settings.UserAuthentication) // Check against the users in Plex
                {
                    Log.Debug("Need to auth");
                    authenticated = CheckIfUserIsInPlexFriends(username, plexSettings.PlexAuthToken);
                    if (authenticated)
                    {
                        userId = GetUserIdIsInPlexFriends(username, plexSettings.PlexAuthToken);
                    }
                    if (CheckIfUserIsOwner(plexSettings.PlexAuthToken, username))
                    {
                        Log.Debug("User is the account owner");
                        authenticated = true;
                        isOwner = true;
                        userId = GetOwnerId(plexSettings.PlexAuthToken, username);
                    }
                    Log.Debug("Friends list result = {0}", authenticated);
                }
                else if (!settings.UserAuthentication) // No auth, let them pass!
                {
                    authenticated = true;
                }
            }
            if (embySettings.Enable)
            {
                if (settings.UserAuthentication) // Check against the users in Plex
                {
                    Log.Debug("Need to auth");
                    authenticated = CheckIfEmbyUser(username, embySettings);
                    if (authenticated)
                    {
                        embyUser = GetEmbyUser(username, embySettings);
                        userId = embyUser?.Id;
                    }
                    if (embyUser?.Policy?.IsAdministrator ?? false)
                    {
                        Log.Debug("User is the account owner");
                        authenticated = true;
                        isOwner = true;
                    }
                    Log.Debug("Friends list result = {0}", authenticated);
                }
                else if (!settings.UserAuthentication) // No auth, let them pass!
                {
                    authenticated = true;
                }
            }

            UsersModel dbUser = await IsDbuser(username);
            if (dbUser != null) // in the db?
            {
                var perms = (Permissions)dbUser.Permissions;
                authenticated = true;
                isOwner = perms.HasFlag(Permissions.Administrator);
                userId = dbUser.UserGuid;
            }

            if (settings.UsePassword || isOwner || Security.HasPermissions(username, Permissions.Administrator))
            {
                Session[SessionKeys.UserLoginName] = username;

                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Views", "UserLogin");
                var file = System.IO.Directory.GetFiles(path).FirstOrDefault(x => x.Contains("Password.cshtml"));
                var html = File.ReadAllText(file);

                return Response.AsJson(new { result = true, usePassword = true, html });
            }

            if (!authenticated)
            {
                return Response.AsJson(new { result = false, message = Resources.UI.UserLogin_IncorrectUserPass });
            }
            var result = await AuthenticationSetup(userId, username, dateTimeOffset, loginGuid, isOwner, plexSettings.Enable, embySettings.Enable);


            var landingSettings = await LandingPageSettings.GetSettingsAsync();

            if (landingSettings.Enabled)
            {
                if (!landingSettings.BeforeLogin) // After Login
                {
                    var uri = Linker.BuildRelativeUri(Context, "LandingPageIndex");
                    if (loginGuid != Guid.Empty)
                    {
                        return CustomModuleExtensions.LoginAndRedirect(this, result.LoginGuid, null, uri.ToString());
                    }
                    return Response.AsRedirect(uri.ToString());
                }
            }


            var retVal = Linker.BuildRelativeUri(Context, "SearchIndex");
            if (result.LoginGuid != Guid.Empty)
            {
                return CustomModuleExtensions.LoginAndRedirect(this, result.LoginGuid, null, retVal.ToString());
            }
            return Response.AsJson(new { result = true, url = retVal.ToString() });
        }

        private async Task<PlexUsers> IsPlexUser(string username)
        {
            var plexLocalUsers = await PlexUserRepository.GetAllAsync();

            var plexLocal = plexLocalUsers.FirstOrDefault(x => x.Username == username);
            return plexLocal;
        }

        private async Task<UsersModel> IsDbuser(string username)
        {
            var localUsers = await CustomUserMapper.GetUsersAsync();

            var dbUser = localUsers.FirstOrDefault(x => x.UserName == username);
            return dbUser;
        }

        private async Task<Response> PasswordLogin()
        {
            var password = Request.Form.password.Value;

            if (string.IsNullOrEmpty(password))
            {

                return Response.AsJson(new { result = false, message = Resources.UI.UserLogin_IncorrectUserPass });
            }

            var dateTimeOffset = Request.Form.DateTimeOffset;
            var loginGuid = Guid.Empty;
            var settings = await AuthService.GetSettingsAsync();
            var username = Session[SessionKeys.UserLoginName].ToString();
            var authenticated = false;
            var isOwner = false;
            var userId = string.Empty;

            var plexSettings = await PlexSettings.GetSettingsAsync();
            var embySettings = await EmbySettings.GetSettingsAsync();

            // attempt local login first as it has the least amount of overhead
            userId = CustomUserMapper.ValidateUser(username, password)?.ToString();
            if (userId != null)
            {
                authenticated = true;
            }
            else if (userId == null && plexSettings.Enable)
            {
                if (settings.UserAuthentication) // Authenticate with Plex
                {
                    Log.Debug("Need to auth and also provide pass");
                    var signedIn = (PlexAuthentication) PlexApi.SignIn(username, password);
                    if (signedIn.user?.authentication_token != null)
                    {
                        Log.Debug("Correct credentials, checking if the user is account owner or in the friends list");
                        if (CheckIfUserIsOwner(plexSettings.PlexAuthToken, signedIn.user?.username))
                        {
                            Log.Debug("User is the account owner");
                            authenticated = true;
                            isOwner = true;
                        }
                        else
                        {
                            authenticated = CheckIfUserIsInPlexFriends(username, plexSettings.PlexAuthToken);
                            Log.Debug("Friends list result = {0}", authenticated);
                        }
                        userId = signedIn.user.uuid;
                    }
                }
            }
            else if (userId == null && embySettings.Enable)
            {
                if (settings.UserAuthentication) // Authenticate with Emby
                {
                    Log.Debug("Need to auth and also provide pass");
                    EmbyUser signedIn = null;
                    try
                    {
                        signedIn = (EmbyUser)EmbyApi.LogIn(username, password, embySettings.ApiKey, embySettings.FullUri);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                    if (signedIn != null)
                    {
                        Log.Debug("Correct credentials, checking if the user is account owner or in the friends list");
                        if (signedIn?.Policy?.IsAdministrator ?? false)
                        {
                            Log.Debug("User is the account owner");
                            authenticated = true;
                            isOwner = true;
                        }
                        else
                        {
                            authenticated = CheckIfEmbyUser(username, embySettings);
                            Log.Debug("Friends list result = {0}", authenticated);
                        }
                        userId = signedIn?.Id;
                    }
                }
            }

            if (!authenticated)
            {
                return Response.AsJson(new { result = false, message = Resources.UI.UserLogin_IncorrectUserPass });
            }

            var m = await AuthenticationSetup(userId, username, dateTimeOffset, loginGuid, isOwner, plexSettings.Enable, embySettings.Enable);

            var landingSettings = await LandingPageSettings.GetSettingsAsync();

            if (landingSettings.Enabled)
            {
                if (!landingSettings.BeforeLogin) // After Login
                {
                    var uri = Linker.BuildRelativeUri(Context, "LandingPageIndex");
                    if (m.LoginGuid != Guid.Empty)
                    {
                        return CustomModuleExtensions.LoginAndRedirect(this, m.LoginGuid, null, uri.ToString());
                    }
                    return Response.AsRedirect(uri.ToString());
                }
            }

            var retVal = Linker.BuildRelativeUri(Context, "SearchIndex");
            if (m.LoginGuid != Guid.Empty)
            {
                return CustomModuleExtensions.LoginAndRedirect(this, m.LoginGuid, null, retVal.ToString());
            }
            return Response.AsJson(new { result = true, url = retVal.ToString() });
        }

        private async Task<Response> LoginUser()
        {
            var userId = string.Empty;
            var loginGuid = Guid.Empty;
            var dateTimeOffset = Request.Form.DateTimeOffset;
            var username = Request.Form.username.Value;
            Log.Debug("Username \"{0}\" attempting to login", username);
            if (string.IsNullOrWhiteSpace(username))
            {
                Session["TempMessage"] = Resources.UI.UserLogin_IncorrectUserPass;
                var uri = Linker.BuildRelativeUri(Context, "UserLoginIndex");
                return Response.AsRedirect(uri.ToString());
            }

            var authenticated = false;
            var isOwner = false;

            var settings = await AuthService.GetSettingsAsync();
            var plexSettings = await PlexSettings.GetSettingsAsync();

            if (IsUserInDeniedList(username, settings))
            {
                Log.Debug("User is in denied list, not allowing them to authenticate");
                Session["TempMessage"] = Resources.UI.UserLogin_IncorrectUserPass;
                var uri = Linker.BuildRelativeUri(Context, "UserLoginIndex");
                return Response.AsRedirect(uri.ToString());
            }

            var password = string.Empty;
            if (settings.UsePassword)
            {
                Log.Debug("Using password");
                password = Request.Form.password.Value;
            }

            var localUsers = await CustomUserMapper.GetUsersAsync();
            var plexLocalUsers = await PlexUserRepository.GetAllAsync();


            if (settings.UserAuthentication && settings.UsePassword) // Authenticate with Plex
            {
                Log.Debug("Need to auth and also provide pass");
                var signedIn = (PlexAuthentication)PlexApi.SignIn(username, password);
                if (signedIn.user?.authentication_token != null)
                {
                    Log.Debug("Correct credentials, checking if the user is account owner or in the friends list");
                    if (CheckIfUserIsOwner(plexSettings.PlexAuthToken, signedIn.user?.username))
                    {
                        Log.Debug("User is the account owner");
                        authenticated = true;
                        isOwner = true;
                    }
                    else
                    {
                        authenticated = CheckIfUserIsInPlexFriends(username, plexSettings.PlexAuthToken);
                        Log.Debug("Friends list result = {0}", authenticated);
                    }
                    userId = signedIn.user.uuid;
                }
            }
            else if (settings.UserAuthentication) // Check against the users in Plex
            {
                Log.Debug("Need to auth");
                authenticated = CheckIfUserIsInPlexFriends(username, plexSettings.PlexAuthToken);
                if (authenticated)
                {
                    userId = GetUserIdIsInPlexFriends(username, plexSettings.PlexAuthToken);
                }
                if (CheckIfUserIsOwner(plexSettings.PlexAuthToken, username))
                {
                    Log.Debug("User is the account owner");
                    authenticated = true;
                    isOwner = true;
                    userId = GetOwnerId(plexSettings.PlexAuthToken, username);
                }
                Log.Debug("Friends list result = {0}", authenticated);
            }
            else if (!settings.UserAuthentication) // No auth, let them pass!
            {
                Log.Debug("No need to auth");
                authenticated = true;
            }

            if (authenticated)
            {
                UserLogins.Insert(new UserLogins { UserId = userId, Type = UserType.PlexUser, LastLoggedIn = DateTime.UtcNow });
                Log.Debug("We are authenticated! Setting session.");
                // Add to the session (Used in the BaseModules)
                Session[SessionKeys.UsernameKey] = (string)username;
                Session[SessionKeys.ClientDateTimeOffsetKey] = (int)dateTimeOffset;

                var plexLocal = plexLocalUsers.FirstOrDefault(x => x.Username == username);
                if (plexLocal != null)
                {
                    loginGuid = Guid.Parse(plexLocal.LoginId);
                }

                var dbUser = localUsers.FirstOrDefault(x => x.UserName == username);
                if (dbUser != null)
                {
                    loginGuid = Guid.Parse(dbUser.UserGuid);
                }

                if (loginGuid != Guid.Empty)
                {
                    if (!settings.UserAuthentication)// Do not need to auth make admin use login screen for now TODO remove this
                    {
                        if (dbUser != null)
                        {
                            var perms = (Permissions)dbUser.Permissions;
                            if (perms.HasFlag(Permissions.Administrator))
                            {
                                var uri = Linker.BuildRelativeUri(Context, "UserLoginIndex");
                                Session["TempMessage"] = Resources.UI.UserLogin_AdminUsePassword;
                                return Response.AsRedirect(uri.ToString());
                            }
                        }
                        if (plexLocal != null)
                        {
                            var perms = (Permissions)plexLocal.Permissions;
                            if (perms.HasFlag(Permissions.Administrator))
                            {
                                var uri = Linker.BuildRelativeUri(Context, "UserLoginIndex");
                                Session["TempMessage"] = Resources.UI.UserLogin_AdminUsePassword;
                                return Response.AsRedirect(uri.ToString());
                            }
                        }
                    }
                }

                if (loginGuid == Guid.Empty && settings.UserAuthentication)
                {
                    var defaultSettings = UserManagementSettings.GetSettings();
                    loginGuid = Guid.NewGuid();

                    var defaultPermissions = (Permissions)UserManagementHelper.GetPermissions(defaultSettings);
                    if (isOwner)
                    {
                        // If we are the owner, add the admin permission.
                        if (!defaultPermissions.HasFlag(Permissions.Administrator))
                        {
                            defaultPermissions += (int)Permissions.Administrator;
                        }
                    }

                    // Looks like we still don't have an entry, so this user does not exist
                    await PlexUserRepository.InsertAsync(new PlexUsers
                    {
                        PlexUserId = userId,
                        UserAlias = string.Empty,
                        Permissions = (int)defaultPermissions,
                        Features = UserManagementHelper.GetPermissions(defaultSettings),
                        Username = username,
                        EmailAddress = string.Empty, // We don't have it, we will  get it on the next scheduled job run (in 30 mins)
                        LoginId = loginGuid.ToString()
                    });
                }
            }

            if (!authenticated)
            {
                var uri = Linker.BuildRelativeUri(Context, "UserLoginIndex");
                Session["TempMessage"] = Resources.UI.UserLogin_IncorrectUserPass;
                return Response.AsRedirect(uri.ToString());
            }

            var landingSettings = await LandingPageSettings.GetSettingsAsync();

            if (landingSettings.Enabled)
            {
                if (!landingSettings.BeforeLogin)
                {
                    var uri = Linker.BuildRelativeUri(Context, "LandingPageIndex");
                    if (loginGuid != Guid.Empty)
                    {
                        return CustomModuleExtensions.LoginAndRedirect(this, loginGuid, null, uri.ToString());
                    }
                    return Response.AsRedirect(uri.ToString());
                }
            }


            var retVal = Linker.BuildRelativeUri(Context, "SearchIndex");
            if (loginGuid != Guid.Empty)
            {
                return CustomModuleExtensions.LoginAndRedirect(this, loginGuid, null, retVal.ToString());
            }
            return Response.AsRedirect(retVal.ToString());
        }

        private class LoginModel
        {
            public Guid LoginGuid { get; set; }
            public string UserId { get; set; }
        }

        private async Task<LoginModel> AuthenticationSetup(string userId, string username, int dateTimeOffset, Guid loginGuid, bool isOwner, bool plex, bool emby)
        {
            var m = new LoginModel();
            var settings = await AuthService.GetSettingsAsync();

            var localUsers = await CustomUserMapper.GetUsersAsync();
            var plexLocalUsers = await PlexUserRepository.GetAllAsync();
            var embyLocalUsers = await EmbyUserRepository.GetAllAsync();

            var localUser = false;


            Log.Debug("We are authenticated! Setting session.");
            // Add to the session (Used in the BaseModules)
            Session[SessionKeys.UsernameKey] = username;
            Session[SessionKeys.ClientDateTimeOffsetKey] = dateTimeOffset;

            if (plex)
            {
                var plexLocal = plexLocalUsers.FirstOrDefault(x => x.Username == username);
                if (plexLocal != null)
                {
                    loginGuid = Guid.Parse(plexLocal.LoginId);
                }
            }
            if (emby)
            {
                var embyLocal = embyLocalUsers.FirstOrDefault(x => x.Username == username);
                if (embyLocal != null)
                {
                    loginGuid = Guid.Parse(embyLocal.LoginId);
                }
            }

            var dbUser = localUsers.FirstOrDefault(x => x.UserName == username);
            if (dbUser != null)
            {
                loginGuid = Guid.Parse(dbUser.UserGuid);
                localUser = true;
            }

            if (loginGuid == Guid.Empty && settings.UserAuthentication)
            {
                var defaultSettings = UserManagementSettings.GetSettings();
                loginGuid = Guid.NewGuid();

                var defaultPermissions = (Permissions)UserManagementHelper.GetPermissions(defaultSettings);
                if (isOwner)
                {
                    // If we are the owner, add the admin permission.
                    if (!defaultPermissions.HasFlag(Permissions.Administrator))
                    {
                        defaultPermissions += (int)Permissions.Administrator;
                    }
                }
                if (plex)
                {
                    // Looks like we still don't have an entry, so this user does not exist
                    await PlexUserRepository.InsertAsync(new PlexUsers
                    {
                        PlexUserId = userId,
                        UserAlias = string.Empty,
                        Permissions = (int) defaultPermissions,
                        Features = UserManagementHelper.GetPermissions(defaultSettings),
                        Username = username,
                        EmailAddress = string.Empty,
                        // We don't have it, we will  get it on the next scheduled job run (in 30 mins)
                        LoginId = loginGuid.ToString()
                    });
                }
                if (emby)
                {
                    await EmbyUserRepository.InsertAsync(new EmbyUsers
                    {
                        EmbyUserId = userId,
                        UserAlias = string.Empty,
                        Permissions = (int)defaultPermissions,
                        Features = UserManagementHelper.GetPermissions(defaultSettings),
                        Username = username,
                        EmailAddress = string.Empty,
                        LoginId = loginGuid.ToString()
                    });
                }
            }
            m.LoginGuid = loginGuid;
            m.UserId = userId;
            var type = UserType.LocalUser;
            if (localUser)
            {
                type = UserType.LocalUser;
            }
            else if (plex)
            {
                type = UserType.PlexUser;
            }
            else if (emby)
            {
                type = UserType.EmbyUser;;
            }
            if (string.IsNullOrEmpty(userId))
            {
                // It's possible we have no auth enabled meaning the userId is empty
                // Let's find that user!

                var user = UserHelper.GetUser(username);
                userId = user.UserId;
            }
            UserLogins.Insert(new UserLogins { UserId = userId, Type = type, LastLoggedIn = DateTime.UtcNow });

            return m;
        }

        private Response Logout()
        {
            if (Session[SessionKeys.UsernameKey] != null)
            {
                Session.Delete(SessionKeys.UsernameKey);
            }
            return Context.GetRedirect(!string.IsNullOrEmpty(BaseUrl)
                ? $"~/{BaseUrl}/userlogin"
                : "~/userlogin");
        }

        private bool CheckIfUserIsOwner(string authToken, string userName)
        {
            var userAccount = PlexApi.GetAccount(authToken);
            if (userAccount == null)
            {
                return false;
            }
            return userAccount.Username != null && userAccount.Username.Equals(userName, StringComparison.CurrentCultureIgnoreCase);
        }

        private string GetOwnerId(string authToken, string userName)
        {
            var userAccount = PlexApi.GetAccount(authToken);
            if (userAccount == null)
            {
                return string.Empty;
            }
            return userAccount.Id;
        }

        private bool CheckIfUserIsInPlexFriends(string username, string authToken)
        {
            var users = PlexApi.GetUsers(authToken);
            var allUsers = users?.User?.Where(x => !string.IsNullOrEmpty(x.Title));
            return allUsers != null && allUsers.Any(x => x.Title.Equals(username, StringComparison.CurrentCultureIgnoreCase));
        }

        private bool CheckIfEmbyUser(string username, EmbySettings s)
        {
            try
            {
                var users = EmbyApi.GetUsers(s.FullUri, s.ApiKey);
                var allUsers = users?.Where(x => !string.IsNullOrEmpty(x.Name));
                return allUsers != null && allUsers.Any(x => x.Name.Equals(username, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }
        }
        private EmbyUser GetEmbyUser(string username, EmbySettings s)
        {
            try
            {

                var users = EmbyApi.GetUsers(s.FullUri, s.ApiKey);
                var allUsers = users?.Where(x => !string.IsNullOrEmpty(x.Name));
                return allUsers?.FirstOrDefault(x => x.Name.Equals(username, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }


        private string GetUserIdIsInPlexFriends(string username, string authToken)
        {
            var users = PlexApi.GetUsers(authToken);
            var allUsers = users?.User?.Where(x => !string.IsNullOrEmpty(x.Title));
            return allUsers?.Where(x => x.Title.Equals(username, StringComparison.CurrentCultureIgnoreCase)).Select(x => x.Id).FirstOrDefault();
        }

        private bool IsUserInDeniedList(string username, AuthenticationSettings settings)
        {
            return settings.DeniedUserList.Any(x => x.Equals(username, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}