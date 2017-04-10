using System.Collections.Generic;
using Ombi.Helpers.Permissions;

namespace Ombi.Core.Users
{
    public interface IUserHelper
    {
        IEnumerable<UserHelperModel> GetUsers();
        IEnumerable<UserHelperModel> GetUsersWithPermission(Permissions permission);
        IEnumerable<UserHelperModel> GetUsersWithFeature(Features feature);
        UserHelperModel GetUser(string username);
    }
}