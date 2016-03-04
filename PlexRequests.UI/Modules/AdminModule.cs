#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: AdminModule.cs
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
using System.Dynamic;
using System.Linq;

using Nancy;
using Nancy.Extensions;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;

using PlexRequests.Api;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class AdminModule : NancyModule
    {
        private ISettingsService<PlexRequestSettings> RpService { get; set; }
        private ISettingsService<CouchPotatoSettings> CpService { get; set; }
        private ISettingsService<AuthenticationSettings> AuthService { get; set; }
        public AdminModule(ISettingsService<PlexRequestSettings> rpService, ISettingsService<CouchPotatoSettings> cpService, ISettingsService<AuthenticationSettings> auth) : base("admin")
        {
            RpService = rpService;
            CpService = cpService;
            AuthService = auth;
#if !DEBUG
            this.RequiresAuthentication();
#endif
            Get["/"] = _ => Admin();

            Get["/authentication"] = _ => Authentication();
            Post["/authentication"] = _ => SaveAuthentication();

            Post["/"] = _ => SaveAdmin();

            Post["/requestauth"] = _ => RequestAuthToken();

            Get["/getusers"] = _ => GetUsers();

            Get["/couchpotato"] = _ => CouchPotato();
            Post["/couchpotato"] = _ => SaveCouchPotato();
        }

        private Negotiator Authentication()
        {
            var settings = AuthService.GetSettings();

            return View["/Authentication", settings];
        }

        private Response SaveAuthentication()
        {
            var model = this.Bind<AuthenticationSettings>();

            var result = AuthService.SaveSettings(model);
            if (result)
            {
                return Context.GetRedirect("~/admin/authentication");
            }
            return Context.GetRedirect("~/error"); //TODO create error page
        }

        private Negotiator Admin()
        {
            dynamic model = new ExpandoObject();
            var settings = RpService.GetSettings();

            model = settings;
            return View["/Settings", model];
        }

        private Response SaveAdmin()
        {
            var model = this.Bind<PlexRequestSettings>();

            RpService.SaveSettings(model);


            return Context.GetRedirect("~/admin");
        }

        private Response RequestAuthToken()
        {
            var user = this.Bind<PlexAuth>();

            if (string.IsNullOrEmpty(user.username) || string.IsNullOrEmpty(user.password))
            {
                return Response.AsJson(new { Result = false, Message = "Please provide a valid username and password" });
            }

            var plex = new PlexApi();
            var model = plex.GetToken(user.username, user.password);

            if (model.user == null)
            {
                return Response.AsJson(new { Result = false, Message = "Incorrect username or password!" });
            }

            var oldSettings = AuthService.GetSettings();
            if (oldSettings != null)
            {
                oldSettings.PlexAuthToken = model.user.authentication_token;
                AuthService.SaveSettings(oldSettings);
            }
            else
            {
                var newModel = new AuthenticationSettings
                {
                    PlexAuthToken = model.user.authentication_token
                };
                AuthService.SaveSettings(newModel);
            }

            return Response.AsJson(new { Result = true, AuthToken = model.user.authentication_token });
        }


        private Response GetUsers()
        {
            var token = AuthService.GetSettings().PlexAuthToken;
            if (token == null)
            {
                return Response.AsJson(string.Empty);
            }
            var api = new PlexApi();
            var users = api.GetUsers(token);
            if (users == null)
            { return Response.AsJson(string.Empty); }

            var usernames = users.User.Select(x => x.Username);
            return Response.AsJson(usernames); //TODO usernames are not populated.
        }

        private Negotiator CouchPotato()
        {
            dynamic model = new ExpandoObject();
            var settings = CpService.GetSettings();
            model = settings;

            return View["/Admin/CouchPotato", model];
        }

        private Response SaveCouchPotato()
        {
            var couchPotatoSettings = this.Bind<CouchPotatoSettings>();

            CpService.SaveSettings(couchPotatoSettings);

            return Context.GetRedirect("~/admin/couchpotato");
        }

    }
}