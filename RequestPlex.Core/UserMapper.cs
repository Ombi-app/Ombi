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

using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;

using RequestPlex.Store;

namespace RequestPlex.Core
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
           var repo = new UserRepository<UserModel>(Db);

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
            var repo = new UserRepository<UserModel>(Db);
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
            var repo = new UserRepository<UserModel>(Db);
            var users = repo.GetAll();
            return users.Any();
        }

        public static Guid? CreateUser(string username, string password)
        {
            var repo = new UserRepository<UserModel>(Db);

            var userModel = new UserModel { UserName = username, User = Guid.NewGuid().ToString(), Password = password };
            repo.Insert(userModel);

            var userRecord = repo.Get(userModel.User);
                
            return new Guid(userRecord.User);
        }
    }
}
