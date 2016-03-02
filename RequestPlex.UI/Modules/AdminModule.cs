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
using System.Web.UI;
using Nancy;
using Nancy.Extensions;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;

using RequestPlex.Api;
using RequestPlex.Core;
using RequestPlex.Core.SettingModels;
using RequestPlex.UI.Models;

namespace RequestPlex.UI.Modules
{
    public class AdminModule : NancyModule
    {
        private ISettingsService<RequestPlexSettings> RpService { get; set; }
        private ISettingsService<CouchPotatoSettings> CpService { get; set; }
        public AdminModule(ISettingsService<RequestPlexSettings> rpService, ISettingsService<CouchPotatoSettings> cpService  ) : base("admin")
        {
            RpService = rpService;
            CpService = cpService;
#if !DEBUG
            this.RequiresAuthentication();
#endif
            Get["/"] = _ => Admin();

            Post["/"] = _ => SaveAdmin();

            Post["/requestauth"] = _ => RequestAuthToken();

            Get["/getusers"] = _ => GetUsers();

            Get["/couchpotato"] = _ => CouchPotato();
            Post["/couchpotato"] = _ => SaveCouchPotato();
        }


        private Negotiator Admin()
        {
            dynamic model = new ExpandoObject();
            var settings = RpService.GetSettings();

            model = settings;
            return View["/Admin/Settings", model];
        }

        private Response SaveAdmin()
        {
            var model = this.Bind<RequestPlexSettings>();

            RpService.SaveSettings(model);


            return Context.GetRedirect("~/admin");
        }

        private Response RequestAuthToken()
        {
            var user = this.Bind<PlexAuth>();

            if (string.IsNullOrEmpty(user.username) || string.IsNullOrEmpty(user.password))
            {
                return Context.GetRedirect("~/admin?error=true");
            }

            var plex = new PlexApi();
            var model = plex.GetToken(user.username, user.password);
            var oldSettings = RpService.GetSettings();
            if (oldSettings != null)
            {
                oldSettings.PlexAuthToken = model.user.authentication_token;
                RpService.SaveSettings(oldSettings);
            }
            else
            {
                var newModel = new RequestPlexSettings
                {
                    PlexAuthToken = model.user.authentication_token
                };
                RpService.SaveSettings(newModel);
            }

            return Response.AsJson(new {Result = true, AuthToken = model.user.authentication_token});
        }


        private Response GetUsers()
        {
            var token = RpService.GetSettings().PlexAuthToken;
            var api = new PlexApi();
            var users = api.GetUsers(token);
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