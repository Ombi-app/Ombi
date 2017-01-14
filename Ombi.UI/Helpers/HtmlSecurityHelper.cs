#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: HtmlSecurityHelper.cs
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
using Nancy.Security;
using Nancy.ViewEngines.Razor;
using Ombi.Helpers.Permissions;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Helpers
{
    public static class HtmlSecurityHelper
    {
        private static ISecurityExtensions Security
        {
            get
            {
                var security = ServiceLocator.Instance.Resolve<ISecurityExtensions>();
                return _security ?? (_security = security);
            }
        }

        private static ISecurityExtensions _security;


        public static bool HasAnyPermission(this HtmlHelpers helper, bool authenticated = true, params Permissions[] permission)
        {
            if (authenticated)
            {
                return helper.CurrentUser.IsAuthenticated()
                       && Security.HasAnyPermissions(helper.CurrentUser, permission);
            }
            return Security.HasAnyPermissions(helper.CurrentUser, permission);
        }

        public static bool DoesNotHavePermission(this HtmlHelpers helper, int permission)
        {
            return Security.DoesNotHavePermissions(permission, helper.CurrentUser);
        }

        public static bool IsAdmin(this HtmlHelpers helper, bool isAuthenticated = true)
        {
            return HasAnyPermission(helper, isAuthenticated, Permissions.Administrator);
        }

        public static bool IsLoggedIn(this HtmlHelpers helper, NancyContext context)
        {
            return Security.IsLoggedIn(context);
        }

        public static bool IsPlexUser(this HtmlHelpers helper)
        {
            return Security.IsPlexUser(helper.CurrentUser);
        }
        public static bool IsNormalUser(this HtmlHelpers helper)
        {
            return Security.IsNormalUser(helper.CurrentUser);
        }
    }
}