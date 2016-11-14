#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: Version1100.cs
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

using System.Data;
using System.Linq;
using PlexRequests.Helpers.Permissions;
using PlexRequests.Store;
using PlexRequests.Store.Repository;

namespace PlexRequests.Core.Migration.Migrations
{
    [Migration(11000, "v1.10.0.0")]
    public class Version1100 : BaseMigration, IMigration
    {
        public Version1100(IUserRepository userRepo)
        {
            UserRepo = userRepo;
        }
        public int Version => 11000;
        private IUserRepository UserRepo { get; }

        public void Start(IDbConnection con)
        {
            UpdateDb(con);

            // Update the current admin permissions set
            UpdateAdmin();

            UpdateSchema(con, Version);
        }

        private void UpdateDb(IDbConnection con)
        {
            // Create the two new columns
            con.AlterTable("Users", "ADD", "Permissions", true, "INTEGER");
            con.AlterTable("Users", "ADD", "Features", true, "INTEGER");

        }

        private void UpdateAdmin()
        {
            var users = UserRepo.GetAll().ToList();

            foreach (var user in users)
            {
                user.Permissions = (int)
                    (Permissions.Administrator
                    | Permissions.ReportIssue
                    | Permissions.RequestMusic
                    | Permissions.RequestTvShow
                    | Permissions.RequestMovie);
            }

            UserRepo.UpdateAll(users);
        }
    }
}