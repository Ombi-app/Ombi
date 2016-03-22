#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: UserMapper.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion
using System;
using System.Linq;
using System.Security;

using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;

using PlexRequests.Helpers;
using PlexRequests.Store;

namespace PlexRequests.Core
{
    public class UserMapper : IUserMapper
    {
        public UserMapper(ISqliteConfiguration db)
        {
            Db = db;
        }
        private static ISqliteConfiguration Db { get; set; }
        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
           var repo = new UserRepository<UsersModel>(Db);

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
            var repo = new UserRepository<UsersModel>(Db);
            var users = repo.GetAll();

            foreach (var u in users)
            {
                if (username == u.UserName)
                {
                    var passwordMatch = PasswordHasher.VerifyPassword(password, u.Salt, u.Hash);
                    if (passwordMatch)
                    {
                        return new Guid(u.UserGuid);
                    }
                }
            }
            return null;
        }

        public static bool DoUsersExist()
        {
            var repo = new UserRepository<UsersModel>(Db);
            var users = repo.GetAll();
            
            return users.Any();
        }

        public static Guid? CreateUser(string username, string password)
        {
            var repo = new UserRepository<UsersModel>(Db);
            var salt = PasswordHasher.GenerateSalt();

            var userModel = new UsersModel { UserName = username, UserGuid = Guid.NewGuid().ToString(), Salt = salt, Hash = PasswordHasher.ComputeHash(password, salt)};
            repo.Insert(userModel);

            var userRecord = repo.Get(userModel.UserGuid);
                
            return new Guid(userRecord.UserGuid);
        }

        public static bool UpdateUser(string username, string oldPassword, string newPassword)
        {
            var repo = new UserRepository<UsersModel>(Db);
            var users = repo.GetAll();
            var userToChange = users.FirstOrDefault(x => x.UserName == username);
            if (userToChange == null)
                return false;

            var passwordMatch = PasswordHasher.VerifyPassword(oldPassword, userToChange.Salt, userToChange.Hash);
            if (!passwordMatch)
            {
                throw new SecurityException("Password does not match");
            }

            var newSalt = PasswordHasher.GenerateSalt();
            var newHash = PasswordHasher.ComputeHash(newPassword, newSalt);

            userToChange.Hash = newHash;
            userToChange.Salt = newSalt;

            return repo.Update(userToChange);
        }
    }
}
