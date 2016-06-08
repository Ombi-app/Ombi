#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: LandingPageModule.cs
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
using System.Threading.Tasks;

using Nancy;
using Nancy.Responses.Negotiation;

using PlexRequests.Api.Interfaces;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.UI.Modules
{
    public class LandingPageModule : BaseModule
    {
        public LandingPageModule(ISettingsService<PlexRequestSettings> settingsService, ISettingsService<LandingPageSettings> landing,
            ISettingsService<PlexSettings> ps, IPlexApi pApi, ISettingsService<AuthenticationSettings> auth) : base("landing", settingsService)
        {
            LandingSettings = landing;
            PlexSettings = ps;
            PlexApi = pApi;
            AuthSettings = auth;

            Get["/", true] = async (x, ct) => await Index();
            Get["/status", true] = async (x, ct) => await CheckStatus();
        }

        private ISettingsService<LandingPageSettings> LandingSettings { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<AuthenticationSettings> AuthSettings { get; }
        private IPlexApi PlexApi { get; }

        private async Task<Negotiator> Index()
        {
            var model = await LandingSettings.GetSettingsAsync();
            return View["Index", model];
        }

        private async Task<Response> CheckStatus()
        {
            var auth = await AuthSettings.GetSettingsAsync();
            var plexSettings = await PlexSettings.GetSettingsAsync();
            if (string.IsNullOrEmpty(auth.PlexAuthToken) || string.IsNullOrEmpty(plexSettings.Ip))
            {
                return Response.AsJson(false);
            }
            try
            {
                var status = PlexApi.GetStatus(auth.PlexAuthToken, plexSettings.FullUri);
                return Response.AsJson(status != null);
            }
            catch (Exception)
            {
                return Response.AsJson(false);
            }

        }
    }
}