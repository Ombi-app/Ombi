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
using System.Linq;
using System.Threading.Tasks;

using Nancy;
using Nancy.Extensions;
using Nancy.Responses.Negotiation;

using NLog;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Plex;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Helpers.Analytics;
using PlexRequests.UI.Models;

using Action = PlexRequests.Helpers.Analytics.Action;

namespace PlexRequests.UI.Modules
{
    public class UserLoginModule : BaseModule
    {
        public UserLoginModule(ISettingsService<AuthenticationSettings> auth, IPlexApi api, ISettingsService<PlexRequestSettings> pr, ISettingsService<LandingPageSettings> lp, IAnalytics a) : base("userlogin", pr)
        {
            AuthService = auth;
            LandingPageSettings = lp;
            Analytics = a;
            Api = api;
            Get["/", true] = async (x, ct) => await Index();
            Post["/"] = x => LoginUser();
            Get["/logout"] = x => Logout();
        }

        private ISettingsService<AuthenticationSettings> AuthService { get; }
        private ISettingsService<LandingPageSettings> LandingPageSettings { get; }
        private IPlexApi Api { get; }
        private IAnalytics Analytics { get; }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        public async Task<Negotiator> Index()
        {
            var query = Request.Query["landing"];
            var landingCheck = (bool?)query ?? true;
            if (landingCheck)
            {
                var landingSettings = await LandingPageSettings.GetSettingsAsync();

                if (landingSettings.Enabled)
                {

                    if (landingSettings.BeforeLogin)
                    {
#pragma warning disable 4014
                        Analytics.TrackEventAsync(
#pragma warning restore 4014
                                 Category.LandingPage,
                                 Action.View,
                                 "Going To LandingPage before login",
                                 Username,
                                 CookieHelper.GetAnalyticClientId(Cookies));

                        var model = new LandingPageViewModel
                        {
                            Enabled = landingSettings.Enabled,
                            Id = landingSettings.Id,
                            EnabledNoticeTime = landingSettings.EnabledNoticeTime,
                            NoticeEnable = landingSettings.NoticeEnable,
                            NoticeEnd = landingSettings.NoticeEnd,
                            NoticeMessage = landingSettings.NoticeMessage,
                            NoticeStart = landingSettings.NoticeStart,
                            ContinueUrl = landingSettings.BeforeLogin ? $"userlogin" : $"search"
                        };

                        return View["Landing/Index", model];
                    }
                }
            }
            var settings = await AuthService.GetSettingsAsync();
            return View["Index", settings];
        }

        private Response LoginUser()
        {
            var dateTimeOffset = Request.Form.DateTimeOffset;
            var username = Request.Form.username.Value;
            Log.Debug("Username \"{0}\" attempting to login", username);
            if (string.IsNullOrWhiteSpace(username))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Incorrect User or Password" });
            }

            var authenticated = false;

            var settings = AuthService.GetSettings();

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
            else if (settings.UserAuthentication) // Check against the users in Plex
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
            else if (!settings.UserAuthentication) // No auth, let them pass!
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

            if (!authenticated)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Incorrect User or Password" });
            }

            var landingSettings = LandingPageSettings.GetSettings();

            if (landingSettings.Enabled)
            {
                if (!landingSettings.BeforeLogin)
                    return Response.AsJson(new JsonResponseModel { Result = true, Message = "landing" });
            }
            return Response.AsJson(new JsonResponseModel { Result = true, Message = "search" });
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
            var allUsers = users?.User?.Where(x => !string.IsNullOrEmpty(x.Title));
            return allUsers != null && allUsers.Any(x => x.Title.Equals(username, StringComparison.CurrentCultureIgnoreCase));
        }

        private bool IsUserInDeniedList(string username, AuthenticationSettings settings)
        {
            return settings.DeniedUserList.Any(x => x.Equals(username));
        }
    }
}