using System;
using System.Linq;

using Mono.Data.Sqlite;

using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;

using RequestPlex.Store;

namespace RequestPlex.Core
{
    public class UserMapper : IUserMapper
    {
        
        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new UserRepository<UserModel>(db);

            var user = repo.Get(identifier.ToString());

            if (user == null)
            {
                return null;
            }

            return new UserIdentity
            {
                UserName = user.UserName,
            };
        }

        public static Guid? ValidateUser(string username, string password)
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new UserRepository<UserModel>(db);
            var users = repo.GetAll();
            var userRecord = users.FirstOrDefault(u => u.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase) && u.Password.Equals(password)); // TODO hashing

            if (userRecord == null)
            {
                return null;
            }

            return new Guid(userRecord.User);
        }

        public static bool DoUsersExist()
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new UserRepository<UserModel>(db);
            var users = repo.GetAll();
            return users.Any();
        }

        public static Guid? CreateUser(string username, string password)
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new UserRepository<UserModel>(db);

            var userModel = new UserModel { UserName = username, User = Guid.NewGuid().ToString(), Password = password };
            repo.Insert(userModel);

            var userRecord = repo.Get(userModel.User);
                
            return new Guid(userRecord.User);
        }
    }
}
