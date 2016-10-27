#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SecurityExtensions.cs
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
using Nancy;
using Nancy.Extensions;
using Nancy.Security;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Helpers
{
    public static class SecurityExtensions
    {

        public static bool IsLoggedIn(this NancyContext context)
        {
            var userName = context.Request.Session[SessionKeys.UsernameKey];
            var realUser = false;
            var plexUser = userName != null;

            if (context.CurrentUser?.IsAuthenticated() ?? false)
            {
                realUser = true;
            }
            
            return realUser || plexUser;
        }

        public static bool IsPlexUser(this NancyContext context)
        {
            var userName = context.Request.Session[SessionKeys.UsernameKey];
            var plexUser = userName != null;

            var isAuth = context.CurrentUser?.IsAuthenticated() ?? false;

            return plexUser && !isAuth;
        }

        public static bool IsNormalUser(this NancyContext context)
        {
            var userName = context.Request.Session[SessionKeys.UsernameKey];
            var plexUser = userName != null;

            var isAuth = context.CurrentUser?.IsAuthenticated() ?? false;

            return isAuth && !plexUser;
        }

        /// <summary>
        /// This module requires authentication and NO certain claims to be present.
        /// </summary>
        /// <param name="module">Module to enable</param>
        /// <param name="requiredClaims">Claim(s) required</param>
        public static void DoesNotHaveClaim(this INancyModule module, params string[] bannedClaims)
        {
            module.AddBeforeHookOrExecute(SecurityHooks.RequiresAuthentication(), "Requires Authentication");
            module.AddBeforeHookOrExecute(DoesNotHaveClaims(bannedClaims), "Has Banned Claims");
        }

        public static bool DoesNotHaveClaimCheck(this INancyModule module, params string[] bannedClaims)
        {
            if (!module.Context?.CurrentUser?.IsAuthenticated() ?? false)
            {
                return false;
            }
            if (DoesNotHaveClaims(bannedClaims, module.Context))
            {
                return false;
            }
            return true;
        }

        public static bool DoesNotHaveClaimCheck(this NancyContext context, params string[] bannedClaims)
        {
            if (!context?.CurrentUser?.IsAuthenticated() ?? false)
            {
                return false;
            }
            if (DoesNotHaveClaims(bannedClaims, context))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Creates a hook to be used in a pipeline before a route handler to ensure
        /// that the request was made by an authenticated user does not have the claims.
        /// </summary>
        /// <param name="claims">Claims the authenticated user needs to have</param>
        /// <returns>Hook that returns an Unauthorized response if the user is not
        /// authenticated or does have the claims, null otherwise</returns>
        private static Func<NancyContext, Response> DoesNotHaveClaims(IEnumerable<string> claims)
        {
            return ForbiddenIfNot(ctx => !ctx.CurrentUser.HasAnyClaim(claims));
        }

        public static bool DoesNotHaveClaims(IEnumerable<string> claims, NancyContext ctx)
        {
            return !ctx.CurrentUser.HasAnyClaim(claims);
        }

        public static bool DoesNotHaveClaims(IEnumerable<string> claims, IUserIdentity identity)
        {
            return !identity?.HasAnyClaim(claims) ?? true;
        }


        // BELOW IS A COPY FROM THE SecurityHooks CLASS!

        /// <summary>
        /// Creates a hook to be used in a pipeline before a route handler to ensure that
        /// the request satisfies a specific test.
        /// </summary>
        /// <param name="test">Test that must return true for the request to continue</param>
        /// <returns>Hook that returns an Forbidden response if the test fails, null otherwise</returns>
        private static Func<NancyContext, Response> ForbiddenIfNot(Func<NancyContext, bool> test)
        {
            return HttpStatusCodeIfNot(HttpStatusCode.Forbidden, test);
        }

        /// <summary>
        /// Creates a hook to be used in a pipeline before a route handler to ensure that
        /// the request satisfies a specific test.
        /// </summary>
        /// <param name="statusCode">HttpStatusCode to use for the response</param>
        /// <param name="test">Test that must return true for the request to continue</param>
        /// <returns>Hook that returns a response with a specific HttpStatusCode if the test fails, null otherwise</returns>
        private static Func<NancyContext, Response> HttpStatusCodeIfNot(HttpStatusCode statusCode, Func<NancyContext, bool> test)
        {
            return ctx =>
            {
                Response response = null;
                if (!test(ctx))
                    response = new Response
                    {
                        StatusCode = statusCode
                    };
                return response;
            };
        }

    }
}