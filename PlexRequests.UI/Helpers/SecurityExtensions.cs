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
using Nancy.Linker;
using Nancy.Responses;
using Nancy.Security;
using Ninject;
using PlexRequests.Helpers.Permissions;
using PlexRequests.Store.Repository;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Helpers
{
    public class SecurityExtensions
    {
        public SecurityExtensions(IUserRepository userRepository, NancyModule context, IResourceLinker linker)
        {
            UserRepository = userRepository;
            Module = context;
            Linker = linker;
        }
        
        private IUserRepository UserRepository { get; }
        private NancyModule Module { get; }
        private IResourceLinker Linker { get; }

        public bool IsLoggedIn(NancyContext context)
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

        public bool IsPlexUser(NancyContext context)
        {
            var userName = context.Request.Session[SessionKeys.UsernameKey];
            var plexUser = userName != null;

            var isAuth = context.CurrentUser?.IsAuthenticated() ?? false;

            return plexUser && !isAuth;
        }

        public bool IsNormalUser(NancyContext context)
        {
            var userName = context.Request.Session[SessionKeys.UsernameKey];
            var plexUser = userName != null;

            var isAuth = context.CurrentUser?.IsAuthenticated() ?? false;

            return isAuth && !plexUser;
        }


        /// <summary>
        /// Creates a hook to be used in a pipeline before a route handler to ensure
        /// that the request was made by an authenticated user does not have the claims.
        /// </summary>
        /// <param name="perm">Claims the authenticated user needs to have</param>
        /// <returns>Hook that returns an Unauthorized response if the user is not
        /// authenticated or does have the claims, null otherwise</returns>
        private Func<NancyContext, Response> DoesNotHavePermissions(int perm)
        { 
            return ForbiddenIfNot(ctx =>
            {
                var dbUser = UserRepository.GetUserByUsername(ctx.CurrentUser.UserName);

                if (dbUser == null) return false;

                var permissions = (Permissions)dbUser.Permissions;
                var result = permissions.HasFlag((Permissions)perm);
                return !result;
            });
        }

        public bool DoesNotHavePermissions(int perm, IUserIdentity currentUser)
        {
            return DoesNotHavePermissions((Permissions) perm, currentUser);
        }

        public bool DoesNotHavePermissions(Permissions perm, IUserIdentity currentUser)
        {
            var dbUser = UserRepository.GetUserByUsername(currentUser.UserName);

            if (dbUser == null) return false;

            var permissions = (Permissions)dbUser.Permissions;
            var result = permissions.HasFlag(perm);
            return !result;
        }

        public bool HasPermissions(IUserIdentity user, Permissions perm)
        {
            if (user == null) return false;

            var dbUser = UserRepository.GetUserByUsername(user.UserName);

            if (dbUser == null) return false;

            var permissions = (Permissions)dbUser.Permissions;
            var result = permissions.HasFlag(perm);
            return result;
        }

        public bool HasAnyPermissions(IUserIdentity user, params Permissions[] perm)
        {
            if (user == null) return false;

            var dbUser = UserRepository.GetUserByUsername(user.UserName);

            if (dbUser == null) return false;

            var permissions = (Permissions)dbUser.Permissions;
            foreach (var p in perm)
            {
                var result = permissions.HasFlag(p);
                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        public Response HasPermissionsRedirect(Permissions perm, NancyContext context, string routeName, HttpStatusCode code)
        {
            var url = Linker.BuildRelativeUri(context, routeName);

            var response = ForbiddenIfNot(ctx =>
            {
                if (ctx.CurrentUser == null) return false;

                var dbUser = UserRepository.GetUserByUsername(ctx.CurrentUser.UserName);

                if (dbUser == null) return false;

                var permissions = (Permissions) dbUser.Permissions;
                var result = permissions.HasFlag(perm);
                return result;
            });

            var r = response(context);
            return r.StatusCode == code
                ? new RedirectResponse(url.ToString()) 
                : null;
        }


        public Response AdminLoginRedirect(Permissions perm, NancyContext context)
        {
            // This will redirect us to the Login Page if we don't have the correct permission passed in (e.g. Admin with Http 403 status code).
            return HasPermissionsRedirect(perm, context, "LocalLogin", HttpStatusCode.Forbidden);
        }
        
        // BELOW IS A COPY FROM THE SecurityHooks CLASS!

        /// <summary>
        /// Creates a hook to be used in a pipeline before a route handler to ensure that
        /// the request satisfies a specific test.
        /// </summary>
        /// <param name="test">Test that must return true for the request to continue</param>
        /// <returns>Hook that returns an Forbidden response if the test fails, null otherwise</returns>
        public Func<NancyContext, Response> ForbiddenIfNot(Func<NancyContext, bool> test)
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
        public Func<NancyContext, Response> HttpStatusCodeIfNot(HttpStatusCode statusCode, Func<NancyContext, bool> test)
        {
            return ctx =>
            {
                Response response = new Response
                {
                    StatusCode = HttpStatusCode.OK
                };
                if (!test(ctx))
                {
                    response = new Response
                    {
                        StatusCode = statusCode
                    };
                }
                return response;
            };
        }

    }
}