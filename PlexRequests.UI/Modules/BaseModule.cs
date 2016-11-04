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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Nancy;
using Ninject;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Helpers.Permissions;
using PlexRequests.Store;
using PlexRequests.Store.Repository;
using PlexRequests.UI.Helpers;
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

            Before += (ctx) => SetCookie();
        }

        protected BaseModule(string modulePath, ISettingsService<PlexRequestSettings> settingsService)
        {

            var settings = settingsService.GetSettings();
            var baseUrl = settings.BaseUrl;
            BaseUrl = baseUrl;

            var settingModulePath = string.IsNullOrEmpty(baseUrl) ? modulePath : $"{baseUrl}/{modulePath}";
    
            ModulePath = settingModulePath;

            Before += (ctx) =>
            {
                SetCookie();

                if (!string.IsNullOrEmpty(ctx.Request.Session["TempMessage"] as string))
                {
                    ctx.ViewBag.TempMessage = ctx.Request.Session["TempMessage"];
                    ctx.ViewBag.TempType = ctx.Request.Session["TempType"];
                    ctx.Request.Session.DeleteAll();
                }
                return null;
            };
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
                    try
                    {
                        _username = Session[SessionKeys.UsernameKey].ToString();
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }
                }
                return _username;
            }
        }

        protected IDictionary<string, string> Cookies => Request?.Cookies;

        // This is not ideal, but it's cleaner than having to pass it down through each module.
        [Inject]
        protected IUserRepository UserRepository { get; set; }

        protected bool IsAdmin
        {
            get
            {
                if (!LoggedIn)
                {
                    return false;
                }

                var user = UserRepository.GetUserByUsername(Context?.CurrentUser?.UserName);

                if (user == null) return false;

                var permissions = (Permissions) user.Permissions;
                return permissions.HasFlag(Permissions.Administrator);
                // TODO: Check admin role
            }
        }

        protected bool LoggedIn => Context?.CurrentUser != null;

        protected string Culture { get; set; }
        protected const string CultureCookieName = "_culture";
        protected Response SetCookie()
        {
            try
            {
                string cultureName;

                // Attempt to read the culture cookie from Request
                var outCookie = string.Empty;
                if (Cookies.TryGetValue(CultureCookieName, out outCookie))
                {
                    cultureName = outCookie;
                }
                else
                {
                    cultureName = Request.Headers?.AcceptLanguage?.FirstOrDefault()?.Item1;
                }

                // Validate culture name
                cultureName = CultureHelper.GetImplementedCulture(cultureName); // This is safe


                // Modify current thread's cultures            
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

                Culture = Thread.CurrentThread.CurrentCulture.Name;
            }
            catch (Exception)
            {
                // Couldn't Set the culture
            }
            return null;
        }

    }
}