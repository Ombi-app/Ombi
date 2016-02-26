using System;

using Dapper.Contrib.Extensions;

namespace RequestPlex.Store
{
    [Table("User")]
    public class UserModel : Entity
    {
        public string User { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
