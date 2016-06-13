#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: BaseModule.cs
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

using Nancy;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public abstract class BaseModule : NancyModule
    {
        protected string BaseUrl { get; set; }

        protected BaseModule(ISettingsService<PlexRequestSettings> settingsService)
        {
            var settings = settingsService.GetSettings();
            var baseUrl = settings.BaseUrl;
            BaseUrl = baseUrl;

            var modulePath = string.IsNullOrEmpty(baseUrl) ? string.Empty : baseUrl;

            ModulePath = modulePath;
        }

        protected BaseModule(string modulePath, ISettingsService<PlexRequestSettings> settingsService) 
        {
            var settings = settingsService.GetSettings();
            var baseUrl = settings.BaseUrl;
            BaseUrl = baseUrl;

            var settingModulePath = string.IsNullOrEmpty(baseUrl) ? modulePath : $"{baseUrl}/{modulePath}";

            ModulePath = settingModulePath;
        }

        private int _dateTimeOffset = -1;
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
        private string _username;

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
                return claims.Contains(UserClaims.Admin) || claims.Contains(UserClaims.PowerUser);
            }
        }

    }
}