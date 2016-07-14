using System.Collections.Generic;

namespace PlexRequests.UI.Models
{
    public class UserManagementUsersViewModel
    {
        public UserManagementUsersViewModel()
        {
            PlexInfo = new UserManagementPlexInformation();
        }
        public string Username { get; set; }
        public string Claims { get; set; }
        public string Id { get; set; }
        public string Alias { get; set; }
        public UserType Type { get; set; }
        public string EmailAddress { get; set; }
        public UserManagementPlexInformation PlexInfo { get; set; }
    }

    public class UserManagementPlexInformation
    {
        public UserManagementPlexInformation()
        {
            Servers = new List<UserManagementPlexServers>();
        }
        public string Thumb { get; set; }
        public List<UserManagementPlexServers> Servers { get; set; }
    }

    public class UserManagementPlexServers
    {
        public int Id { get; set; }
        public string ServerId { get; set; }
        public string MachineIdentifier { get; set; }
        public string Name { get; set; }
        public string LastSeenAt { get; set; }
        public string NumLibraries { get; set; }
    }

    public enum UserType
    {
        PlexUser,
        LocalUser
    }
}

