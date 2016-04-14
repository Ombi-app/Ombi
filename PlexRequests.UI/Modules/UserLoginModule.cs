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
using System.Collections.Generic;
using System.Linq;

using Nancy;
using Nancy.Extensions;
using Nancy.Responses.Negotiation;

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Plex;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class UserLoginModule : BaseModule
    {
        public UserLoginModule(ISettingsService<AuthenticationSettings> auth, IPlexApi api) : base("userlogin")
        {
            AuthService = auth;
            Api = api;
            Get["/"] = _ => Index();
            Post["/"] = x => LoginUser();
            Get["/logout"] = x => Logout();
        }

        private ISettingsService<AuthenticationSettings> AuthService { get; }
        private IPlexApi Api { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        public Negotiator Index()
        {
            var settings = AuthService.GetSettings();
            return View["Index", settings];
        }

        private Response LoginUser()
        {
            var dateTimeOffset = Request.Form.DateTimeOffset;
            var username = Request.Form.username.Value;
            Log.Debug("Username \"{0}\" attempting to login",username);
            if (string.IsNullOrWhiteSpace(username))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Incorrect User or Password" });
            }

            var authenticated = false;

            var settings = AuthService.GetSettings();
            Log.Debug("Settings: ");
            Log.Debug(settings.DumpJson());

            if (IsUserInDeniedList(username, settings))
            {
                Log.Debug("User is in denied list, not allowing them to authenticate");
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Incorrect User or Password" });
            }

            var password = string.Empty;
            if (settings.UsePassword)
            {
                Log.Debug("Using password");
                password = Request.Form.password.Value;
            }

            
            if (settings.UserAuthentication && settings.UsePassword) // Authenticate with Plex
            {
                Log.Debug("Need to auth and also provide pass");
                var signedIn = (PlexAuthentication)Api.SignIn(username, password);
                if (signedIn.user?.authentication_token != null)
                {
                    Log.Debug("Correct credentials, checking if the user is account owner or in the friends list");
                    if (CheckIfUserIsOwner(settings.PlexAuthToken, signedIn.user?.username))
                    {
                        Log.Debug("User is the account owner");
                        authenticated = true;
                    }
                    else
                    {
                        authenticated = CheckIfUserIsInPlexFriends(username, settings.PlexAuthToken);
                        Log.Debug("Friends list result = {0}", authenticated);
                    }
                }
            }
            else if(settings.UserAuthentication) // Check against the users in Plex
            {
                Log.Debug("Need to auth");
                authenticated = CheckIfUserIsInPlexFriends(username, settings.PlexAuthToken);
                if (CheckIfUserIsOwner(settings.PlexAuthToken, username))
                {
                    Log.Debug("User is the account owner");
                    authenticated = true;
                }
                Log.Debug("Friends list result = {0}", authenticated);
            }
            else if(!settings.UserAuthentication) // No auth, let them pass!
            {
                Log.Debug("No need to auth");
                authenticated = true;
            }

            if (authenticated)
            {
                Log.Debug("We are authenticated! Setting session.");
                // Add to the session (Used in the BaseModules)
                Session[SessionKeys.UsernameKey] = (string)username;
            }

            Session[SessionKeys.ClientDateTimeOffsetKey] = (int)dateTimeOffset;

            return Response.AsJson(authenticated 
                ? new JsonResponseModel { Result = true } 
                : new JsonResponseModel { Result = false, Message = "Incorrect User or Password"});
        }

        

        private Response Logout()
        {
            Log.Debug("Logging Out");
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
            var userAccount = Api.GetAccount(authToken);
            if (userAccount == null)
            {
                return false;
            }
            return userAccount.Username != null && userAccount.Username.Equals(userName, StringComparison.CurrentCultureIgnoreCase);
        }

        private bool CheckIfUserIsInPlexFriends(string username, string authToken)
        {
            var users = Api.GetUsers(authToken);
            Log.Debug("Plex Users: ");
            Log.Debug(users.DumpJson());
            var allUsers = users?.User?.Where(x => !string.IsNullOrEmpty(x.Username));
            return allUsers != null && allUsers.Any(x => x.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
        }

        private bool IsUserInDeniedList(string username, AuthenticationSettings settings)
        {
            return settings.DeniedUserList.Any(x => x.Equals(username));
        }
    }
}