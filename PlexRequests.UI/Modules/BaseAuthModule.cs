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
using System.Linq;


#endregion

using Nancy;
using Nancy.Extensions;
using PlexRequests.UI.Models;
using System;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;

namespace PlexRequests.UI.Modules
{
    public abstract class BaseAuthModule : BaseModule
    {
        private string _username;
        private int _dateTimeOffset = -1;

        protected string Username
        {
            get
            {
                if (string.IsNullOrEmpty(_username))
                {
                    _username = Session[SessionKeys.UsernameKey].ToString();
                }
                return _username;
            }
        }

        protected bool IsAdmin
        {
            get
            {
                if (Context.CurrentUser == null)
                {
                    return false;
                }
                var claims = Context.CurrentUser.Claims.ToList();
                if (claims.Contains(UserClaims.Admin) || claims.Contains(UserClaims.PowerUser))
                {
                    return true;
                }
                return false;
            }
        }

        protected int DateTimeOffset
        {
            get
            {
                if (_dateTimeOffset == -1)
                {
                    _dateTimeOffset = (int?)Session[SessionKeys.ClientDateTimeOffsetKey] ?? new DateTimeOffset().Offset.Minutes;
                }
                return _dateTimeOffset;
            }
        }

        protected BaseAuthModule(ISettingsService<PlexRequestSettings> pr) : base(pr)
        {
            Service = pr;
            Before += (ctx) => CheckAuth();
        }

        protected BaseAuthModule(string modulePath, ISettingsService<PlexRequestSettings> pr) : base(modulePath, pr)
        {
            Service = pr;
            Before += (ctx) => CheckAuth();
        }

        private ISettingsService<PlexRequestSettings> Service { get; }

        private Response CheckAuth()
        {
            var settings = Service.GetSettings();
            var baseUrl = settings.BaseUrl;

            var redirectPath = string.IsNullOrEmpty(baseUrl) ? "~/userlogin" : $"~/{baseUrl}/userlogin";

            return Session[SessionKeys.UsernameKey] == null
                ? Context.GetRedirect(redirectPath)
                : null;
        }
    }
}