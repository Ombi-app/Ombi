using System.Collections.Generic;

namespace Ombi.Core.Models.UI
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Alias { get; set; }
        public List<string> Claims { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public UserType UserType { get; set; }
    }
}
