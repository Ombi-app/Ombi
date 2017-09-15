using System.Collections.Generic;

namespace Ombi.Settings.Settings.Models
{
    public class UserManagementSettings : Settings
    {
        public bool ImportMediaServerUsers { get; set; }
        public List<string> DefaultRoles { get; set; }
    }
}