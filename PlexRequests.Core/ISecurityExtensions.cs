using System;
using Nancy;
using Nancy.Security;
using PlexRequests.Helpers.Permissions;

namespace PlexRequests.Core
{
    public interface ISecurityExtensions
    {
        Response AdminLoginRedirect(Permissions perm, NancyContext context);
        bool DoesNotHavePermissions(Permissions perm, IUserIdentity currentUser);
        bool DoesNotHavePermissions(int perm, IUserIdentity currentUser);
        Func<NancyContext, Response> ForbiddenIfNot(Func<NancyContext, bool> test);
        bool HasAnyPermissions(IUserIdentity user, params Permissions[] perm);
        bool HasPermissions(IUserIdentity user, Permissions perm);
        Response HasPermissionsRedirect(Permissions perm, NancyContext context, string routeName, HttpStatusCode code);
        Func<NancyContext, Response> HttpStatusCodeIfNot(HttpStatusCode statusCode, Func<NancyContext, bool> test);
        bool IsLoggedIn(NancyContext context);
        bool IsNormalUser(NancyContext context);
        bool IsPlexUser(NancyContext context);
        bool HasPermissions(string userName, Permissions perm);
    }
}