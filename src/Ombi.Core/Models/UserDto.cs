using System.Collections.Generic;
using System.Security.Claims;

namespace Ombi.Core.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Alias { get; set; }
        public List<Claim> Claims { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public byte[] Salt { get; set; }
        public UserType UserType { get; set; }
    }

    public enum UserType
    {
        LocalUser = 1,
        PlexUser = 2,
        EmbyUser = 3,
        JellyfinUser = 5
    }
}
