#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CustomModuleExtensions.cs
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
using Nancy;
using Nancy.Extensions;

namespace Ombi.UI.Authentication
{
    public static class CustomModuleExtensions
    {

        /// <summary>
        /// Logs the user in and returns either an empty 200 response for ajax requests, or a redirect response for non-ajax. <seealso cref="M:Nancy.Extensions.RequestExtensions.IsAjaxRequest(Nancy.Request)" />
        /// </summary>
        /// <param name="module">Nancy module</param>
        /// <param name="userIdentifier">User identifier guid</param>
        /// <param name="cookieExpiry">Optional expiry date for the cookie (for 'Remember me')</param>
        /// <param name="fallbackRedirectUrl">Url to redirect to if none in the querystring</param>
        /// <returns>Nancy response with redirect if request was not ajax, otherwise with OK.</returns>
        public static Response Login(this INancyModule module, Guid userIdentifier, DateTime? cookieExpiry = null, string fallbackRedirectUrl = "/")
        {
            if (!module.Context.Request.IsAjaxRequest())
                return module.LoginAndRedirect(userIdentifier, cookieExpiry, fallbackRedirectUrl);
            return module.LoginWithoutRedirect(userIdentifier, cookieExpiry);
        }

        /// <summary>
        /// Logs the user in with the given user guid and redirects.
        /// </summary>
        /// <param name="module">Nancy module</param>
        /// <param name="userIdentifier">User identifier guid</param>
        /// <param name="cookieExpiry">Optional expiry date for the cookie (for 'Remember me')</param>
        /// <param name="fallbackRedirectUrl">Url to redirect to if none in the querystring</param>
        /// <returns>Nancy response instance</returns>
        public static Response LoginAndRedirect(this INancyModule module, Guid userIdentifier, DateTime? cookieExpiry = null, string fallbackRedirectUrl = "/")
        {
            return CustomAuthenticationProvider.UserLoggedInRedirectResponse(module.Context, userIdentifier, cookieExpiry, fallbackRedirectUrl);
        }

        /// <summary>
        /// Logs the user in with the given user guid and returns ok response.
        /// </summary>
        /// <param name="module">Nancy module</param>
        /// <param name="userIdentifier">User identifier guid</param>
        /// <param name="cookieExpiry">Optional expiry date for the cookie (for 'Remember me')</param>
        /// <returns>Nancy response instance</returns>
        public static Response LoginWithoutRedirect(this INancyModule module, Guid userIdentifier, DateTime? cookieExpiry = null)
        {
            return CustomAuthenticationProvider.UserLoggedInResponse(userIdentifier, cookieExpiry);
        }

        /// <summary>
        /// Logs the user out and returns either an empty 200 response for ajax requests, or a redirect response for non-ajax. <seealso cref="M:Nancy.Extensions.RequestExtensions.IsAjaxRequest(Nancy.Request)" />
        /// </summary>
        /// <param name="module">Nancy module</param>
        /// <param name="redirectUrl">URL to redirect to</param>
        /// <returns>Nancy response with redirect if request was not ajax, otherwise with OK.</returns>
        public static Response Logout(this INancyModule module, string redirectUrl)
        {
            if (!module.Context.Request.IsAjaxRequest())
                return CustomAuthenticationProvider.LogOutAndRedirectResponse(module.Context, redirectUrl);
            return CustomAuthenticationProvider.LogOutResponse();
        }

        /// <summary>Logs the user out and redirects</summary>
        /// <param name="module">Nancy module</param>
        /// <param name="redirectUrl">URL to redirect to</param>
        /// <returns>Nancy response instance</returns>
        public static Response LogoutAndRedirect(this INancyModule module, string redirectUrl)
        {
            return CustomAuthenticationProvider.LogOutAndRedirectResponse(module.Context, redirectUrl);
        }

        /// <summary>Logs the user out without a redirect</summary>
        /// <param name="module">Nancy module</param>
        /// <returns>Nancy response instance</returns>
        public static Response LogoutWithoutRedirect(this INancyModule module)
        {
            return CustomAuthenticationProvider.LogOutResponse();
        }
    }
}
