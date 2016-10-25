using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlexRequests.Helpers;

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
        public string[] ClaimsArray { get; set; }
        public List<UserManagementUpdateModel.ClaimsModel> ClaimsItem { get; set; }
        public DateTime LastLoggedIn { get; set; }
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


    public class UserManagementCreateModel
    {
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("claims")]
        public string[] Claims { get; set; }

        [JsonProperty("email")]
        public string EmailAddress { get; set; }
    }

    public class UserManagementUpdateModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("claims")]
        public List<ClaimsModel> Claims { get; set; }

        public string Alias { get; set; }
        public string EmailAddress { get; set; }

        public class ClaimsModel
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("selected")]
            public bool Selected { get; set; }
        }
        
    }
}

