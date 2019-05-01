
using Ombi.Store.Entities;

namespace Ombi.Models
{
    public class ConnectedUsersViewModel
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public UserType UserType { get; set; }
    }
}