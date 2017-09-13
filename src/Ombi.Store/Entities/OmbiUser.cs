using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Ombi.Store.Entities
{
    public class OmbiUser : IdentityUser
    {
        public string Alias { get; set; }
        public UserType UserType { get; set; }

        [NotMapped]
        public string UserAlias => string.IsNullOrEmpty(Alias) ? UserName : Alias;
    }
}