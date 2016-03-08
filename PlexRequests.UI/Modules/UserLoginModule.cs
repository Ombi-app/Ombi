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
using System.Linq;

using Nancy;
using Nancy.Responses.Negotiation;

using PlexRequests.Api;
using PlexRequests.Api.Models;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    // TODO: Check the settings to see if we need to authenticate
    // TODO: Add ability to logout
    // TODO: If we need to authenticate we need to check if they are in Plex
    // TODO: Allow the user of a username only or a Username and password
    public class UserLoginModule : NancyModule
    {
        public UserLoginModule(ISettingsService<AuthenticationSettings> auth) : base("userlogin")
        {
            AuthService = auth;
            Get["/"] = _ => Index();
            Post["/"] = x => LoginUser();
        }

        public Negotiator Index()
        {
            var settings = AuthService.GetSettings();
            return View["Index", settings];
        }
        private ISettingsService<AuthenticationSettings> AuthService { get; set; }

        private Response LoginUser()
        {
            var authenticated = false;
            var api = new PlexApi();
            var settings = AuthService.GetSettings();
            var username = Request.Form.username.Value;
            var password = string.Empty;
            if (settings.UsePassword)
            {
                password = Request.Form.password.Value;
            }

            
            if (settings.UserAuthentication && settings.UsePassword) // Authenticate with Plex
            {

                var signedIn = (PlexAuthentication)api.SignIn(username, password);
                if (signedIn.user?.authentication_token != null)
                {
                    var users = api.GetUsers(settings.PlexAuthToken);
                    if (users.User.Any(x => x.Username == username))
                    {
                        authenticated = true;
                    }
                }
            }
            else if(settings.UserAuthentication) // Check against the users in Plex
            {
                var users = api.GetUsers(settings.PlexAuthToken);
                if (users.User.Any(x => x.Username == username))
                {
                    authenticated = true;
                }
            }
            else if(!settings.UserAuthentication)
            {
                authenticated = true;
            }


            // Add to the session
            Session[SessionKeys.UsernameKey] = (string)username;
            return Response.AsJson(authenticated 
                ? new JsonResponseModel { Result = true } 
                : new JsonResponseModel { Result = false, Message = "Incorrect User or Password"});
        }
    }
}