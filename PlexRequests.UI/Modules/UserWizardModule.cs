#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: UserWizardModule.cs
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
using System.Threading.Tasks;

using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Validation;

using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.UI.Helpers;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class UserWizardModule : BaseModule
    {
        public UserWizardModule(ISettingsService<PlexRequestSettings> pr, ISettingsService<PlexSettings> plex, IPlexApi plexApi,
            ISettingsService<AuthenticationSettings> auth) : base("wizard", pr)
        {
            PlexSettings = plex;
            PlexApi = plexApi;
            PlexRequestSettings = pr;
            Auth = auth;

            Get["/"] = x => Index();
            Post["/plexAuth"] = x => PlexAuth();
            Post["/plex", true] = async (x,ct) => await Plex();
            Post["/plexrequest", true] = async (x,ct) => await PlexRequest();
            Post["/auth", true] = async (x,ct) => await Authentication();
        }

        private ISettingsService<PlexSettings> PlexSettings { get; }
        private IPlexApi PlexApi { get; }
        private ISettingsService<PlexRequestSettings> PlexRequestSettings { get; }
        private ISettingsService<AuthenticationSettings> Auth { get; }

        private Negotiator Index()
        {
            return View["Index"];
        }




        private Response PlexAuth()
        {
            var user = this.Bind<PlexAuth>();

            if (string.IsNullOrEmpty(user.username) || string.IsNullOrEmpty(user.password))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Please provide a valid username and password" });
            }

            var model = PlexApi.SignIn(user.username, user.password);

            if (model?.user == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Incorrect username or password!" });
            }

            // Set the auth token in the session so we can use it in the next form
            Session[SessionKeys.UserWizardPlexAuth] = model.user.authentication_token;

            var servers = PlexApi.GetServer(model.user.authentication_token);
            var firstServer = servers.Server.FirstOrDefault();
            return Response.AsJson(new { Result = true, firstServer?.Port, Ip = firstServer?.LocalAddresses, firstServer?.Scheme });
        }

        private async Task<Response> Plex()
        {
            var form = this.Bind<PlexSettings>();
            var valid = this.Validate(form);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }
            form.PlexAuthToken = Session[SessionKeys.UserWizardPlexAuth].ToString(); // Set the auth token from the previous form

            var result = await PlexSettings.SaveSettingsAsync(form);
            if (result)
            {
                return Response.AsJson(new JsonResponseModel { Result = true });
            }
            return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not save the settings to the database, please try again." });
        }

        private async Task<Response> PlexRequest()
        {
            var form = this.Bind<PlexRequestSettings>();
            var valid = this.Validate(form);
            if (!valid.IsValid)
            {
                return Response.AsJson(valid.SendJsonError());
            }

            var result = await PlexRequestSettings.SaveSettingsAsync(form);
            if (result)
            {
                return Response.AsJson(new { Result = true });
            }

            return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not save the settings to the database, please try again." });
        }

        private async Task<Response> Authentication()
        {
            var form = this.Bind<AuthenticationSettings>();

            var result = await Auth.SaveSettingsAsync(form);
            if (result)
            {
                return Response.AsJson(new JsonResponseModel { Result = true });
            }
            return Response.AsJson(new JsonResponseModel { Result = false, Message = "Could not save the settings to the database, please try again." });
        }
    }
}