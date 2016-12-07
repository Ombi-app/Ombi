using System.Collections.Generic;
using PlexRequests.Helpers.Permissions;

namespace PlexRequests.Core.Users
{
    public interface IUserHelper
    {
        IEnumerable<UserHelperModel> GetUsers();
        IEnumerable<UserHelperModel> GetUsersWithPermission(Permissions permission);
        IEnumerable<UserHelperModel> GetUsersWithFeature(Features feature);
    }
}