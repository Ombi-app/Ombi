
namespace RequestPlex.Api.Models
{
    public class PlexUserRequest
    {
        public UserRequest user { get; set; }
    }

    public class UserRequest
    {
        public string login { get; set; }
        public string password { get; set; }
    }
}
