using System;
using Nancy;
using Nancy.Security;
using Nancy.Session;
using Ombi.Helpers.Permissions;

namespace Ombi.Core
{
    public interface ISecurityExtensions
    {
        Response AdminLoginRedirect(Permissions perm, NancyContext context);
        Response AdminLoginRedirect(NancyContext context, params Permissions[] perm);
        bool DoesNotHavePermissions(Permissions perm, IUserIdentity currentUser);

        Response HasAnyPermissionsRedirect(NancyContext context, string routeName, HttpStatusCode code,
            params Permissions[] perm);
        bool DoesNotHavePermissions(int perm, IUserIdentity currentUser);
        Func<NancyContext, Response> ForbiddenIfNot(Func<NancyContext, bool> test);
        bool HasAnyPermissions(IUserIdentity user, params Permissions[] perm);
        bool HasPermissions(IUserIdentity user, Permissions perm);
        Response HasPermissionsRedirect(Permissions perm, NancyContext context, string routeName, HttpStatusCode code);
        Func<NancyContext, Response> HttpStatusCodeIfNot(HttpStatusCode statusCode, Func<NancyContext, bool> test);
        bool IsLoggedIn(NancyContext context);
        bool IsNormalUser(IUserIdentity user);
        bool IsPlexUser(IUserIdentity user);
        bool HasPermissions(string userName, Permissions perm);

        /// <summary>
        /// Gets the username this could be the alias! We should always use this method when getting the username
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="session">The session.</param>
        /// <returns>
        ///   <c>null</c> if we cannot find a user
        /// </returns>
        string GetUsername(string username, ISession session);
    }
}