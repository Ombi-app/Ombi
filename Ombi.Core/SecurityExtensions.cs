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
using Nancy;
using Nancy.Linker;
using Nancy.Responses;
using Nancy.Security;
using Nancy.Session;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;
using Ombi.Core.Users;
using Ombi.Helpers;
using Ombi.Helpers.Permissions;
using Ombi.Store.Models.Plex;
using Ombi.Store.Repository;

namespace Ombi.Core
{
    public class SecurityExtensions : ISecurityExtensions
    {
        public SecurityExtensions(IUserRepository userRepository, IResourceLinker linker, IExternalUserRepository<PlexUsers> plexUsers, ISettingsService<UserManagementSettings> umSettings)
        {
            UserRepository = userRepository;
            Linker = linker;
            PlexUsers = plexUsers;
            UserManagementSettings = umSettings;
        }

        private IUserRepository UserRepository { get; }
        private IResourceLinker Linker { get; }
        private IExternalUserRepository<PlexUsers> PlexUsers { get; }
        private ISettingsService<UserManagementSettings> UserManagementSettings { get; }

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

        public bool IsPlexUser(IUserIdentity user)
        {
            if (user == null)
            {
                return false;
            }
            var plexUser = PlexUsers.GetUserByUsername(user.UserName);
            return plexUser != null;
        }

        public bool IsNormalUser(IUserIdentity user)
        {
            if (user == null)
            {
                return false;
            }
            var dbUser = UserRepository.GetUserByUsername(user.UserName);

            return dbUser != null;
        }

        /// <summary>
        /// Gets the username this could be the alias! We should always use this method when getting the username
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="session"></param>
        /// <returns>
        ///   <c>null</c> if we cannot find a user
        /// </returns>
        public string GetUsername(string username, ISession session)
        {
            var plexUser = PlexUsers.GetUserByUsername(username);
            if (plexUser != null)
            {
                return !string.IsNullOrEmpty(plexUser.UserAlias) ? plexUser.UserAlias : plexUser.Username;
            }

            var dbUser = UserRepository.GetUserByUsername(username);
            if (dbUser != null)
            {
                var userProps = ByteConverterHelper.ReturnObject<UserProperties>(dbUser.UserProperties);
                return !string.IsNullOrEmpty(userProps.UserAlias) ? userProps.UserAlias : dbUser.UserName;
            }

            // could be a local user
            var hasSessionKey = session[SessionKeys.UsernameKey] != null;
            if (hasSessionKey)
            {
               return (string)session[SessionKeys.UsernameKey];
            }

            return string.Empty;
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
                var permissions = GetPermissions(ctx.CurrentUser);
                var result = permissions.HasFlag((Permissions)perm);
                return !result;
            });
        }

        public bool DoesNotHavePermissions(int perm, IUserIdentity currentUser)
        {
            return DoesNotHavePermissions((Permissions)perm, currentUser);
        }

        public bool DoesNotHavePermissions(Permissions perm, IUserIdentity currentUser)
        {
            var permissions = GetPermissions(currentUser);
            var result = permissions.HasFlag(perm);
            return !result;
        }

        public bool HasPermissions(IUserIdentity user, Permissions perm)
        {
            var permissions = GetPermissions(user);
            return permissions.HasFlag(perm);
        }
        public bool HasPermissions(string userName, Permissions perm)
        {
            var permissions = GetPermissions(userName);
            return permissions.HasFlag(perm);
        }

        public bool HasAnyPermissions(IUserIdentity user, params Permissions[] perm)
        {
            var permissions = GetPermissions(user);

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
                var permissions = GetPermissions(ctx.CurrentUser);
                var result = permissions.HasFlag(perm);
                return result;
            });

            var r = response(context);
            return r.StatusCode == code
                ? new RedirectResponse($"{url.ToString()}?redirect={context.Request.Path}")
                : null;
        }
        public Response HasAnyPermissionsRedirect(NancyContext context, string routeName, HttpStatusCode code, params Permissions[] perm)
        {
            var url = Linker.BuildRelativeUri(context, routeName);

            var response = ForbiddenIfNot(ctx =>
            {
                var permissions = GetPermissions(ctx.CurrentUser);
                var hasPermission = false;
                foreach (var p in perm)
                {
                    var result = permissions.HasFlag(p);
                    if (result)
                    {
                        hasPermission = true;
                    }
                }
                return hasPermission;
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

        public Response AdminLoginRedirect(NancyContext context, params Permissions[] perm)
        {
            // This will redirect us to the Login Page if we don't have the correct permission passed in (e.g. Admin with Http 403 status code).
            return HasAnyPermissionsRedirect(context, "LocalLogin", HttpStatusCode.Forbidden, perm);
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

        private Permissions GetPermissions(IUserIdentity user)
        {
            return GetPermissions(user?.UserName);
        }

        private Permissions GetPermissions(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                // Username without auth
                var s = UserManagementSettings.GetSettings();
                return (Permissions)UserManagementHelper.GetPermissions(s);
            }

            var dbUser = UserRepository.GetUserByUsername(userName);

            if (dbUser != null)
            {
                var permissions = (Permissions)dbUser.Permissions;
                return permissions;
            }

            var plexUser = PlexUsers.GetUserByUsername(userName);
            if (plexUser != null)
            {
                var permissions = (Permissions)plexUser.Permissions;
                return permissions;
            }

            return 0;
        }
    }
}