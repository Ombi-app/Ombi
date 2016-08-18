#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: BaseAuthModule.cs
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

using Nancy;
using Nancy.Extensions;
using PlexRequests.UI.Models;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.UI.Modules
{
    public abstract class BaseAuthModule : BaseModule
    {
        protected BaseAuthModule(ISettingsService<PlexRequestSettings> pr) : base(pr)
        {
            PlexRequestSettings = pr;
            Before += (ctx) => CheckAuth();
        }

        protected BaseAuthModule(string modulePath, ISettingsService<PlexRequestSettings> pr) : base(modulePath, pr)
        {
            PlexRequestSettings = pr;
            Before += (ctx) => CheckAuth();
        }

        protected ISettingsService<PlexRequestSettings> PlexRequestSettings { get; }

        private Response CheckAuth()
        {
            var settings = PlexRequestSettings.GetSettings();
            // Have we been through the wizard?
            if (!settings.Wizard)
            {
                return Context.GetRedirect("~/wizard");
            }

            var baseUrl = settings.BaseUrl;

            var redirectPath = string.IsNullOrEmpty(baseUrl) ? "~/userlogin" : $"~/{baseUrl}/userlogin";

            return Session[SessionKeys.UsernameKey] == null
                ? Context.GetRedirect(redirectPath)
                : null;
        }
    }
}