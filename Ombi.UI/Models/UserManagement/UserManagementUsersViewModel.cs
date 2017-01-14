using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Ombi.Helpers;

namespace Ombi.UI.Models.UserManagement
{
    public class UserManagementUsersViewModel
    {
        public UserManagementUsersViewModel()
        {
            PlexInfo = new UserManagementPlexInformation();
            Permissions = new List<CheckBox>();
            Features = new List<CheckBox>();
        }
        public string Username { get; set; }
        public string FeaturesFormattedString { get; set; }
        public string PermissionsFormattedString { get; set; }
        public string Id { get; set; }
        public string Alias { get; set; }
        public UserType Type { get; set; }
        public string EmailAddress { get; set; }
        public UserManagementPlexInformation PlexInfo { get; set; }
        public DateTime LastLoggedIn { get; set; }
        public List<CheckBox> Permissions { get; set; }
        public List<CheckBox> Features { get; set; }
        public bool ManagedUser { get; set; }
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

    public class CheckBox
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool Selected { get; set; }
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
        [JsonProperty("permissions")]
        public List<string> Permissions { get; set; }
        [JsonProperty("features")]
        public List<string> Features { get; set; }

        [JsonProperty("email")]
        public string EmailAddress { get; set; }
    }

    public class UserManagementUpdateModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("permissions")]
        public List<CheckBox> Permissions { get; set; }
        [JsonProperty("features")]
        public List<CheckBox> Features { get; set; }
        public string Alias { get; set; }
        public string EmailAddress { get; set; }  
    }
}

