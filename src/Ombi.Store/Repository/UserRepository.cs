#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: UserRepository.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    //public class UserRepository : IUserRepository
    //{
    //    public UserRepository(OmbiContext ctx)
    //    {
    //        Db = ctx;
    //    }

    //    private OmbiContext Db { get; }

    //    public async Task<User> GetUser(string username)
    //    {
    //        var user = await Db.OldUsers.FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());
    //        Db.Entry(user).Reload();
    //        return user;
    //    }

    //    public async Task<User> GetUser(int userId)
    //    {
    //        var user = await Db.OldUsers.FirstOrDefaultAsync(x => x.Id == userId);
    //        Db.Entry(user).Reload();
    //        return user;
    //    }

    //    public async Task CreateUser(User user)
    //    {
    //        Db.OldUsers.Add(user);
    //        await Db.SaveChangesAsync();
    //    }

    //    public async Task<IEnumerable<User>> GetUsers()
    //    {
    //        return await Db.OldUsers.ToListAsync();
    //    }

    //    public async Task DeleteUser(User user)
    //    {
    //        Db.OldUsers.Remove(user);
    //        await Db.SaveChangesAsync();
    //    }

    //    public async Task<User> UpdateUser(User user)
    //    {
    //        Db.Entry(user).State = EntityState.Modified;
    //        Db.Entry(user).Property(x => x.Salt).IsModified = false;
    //        Db.Entry(user).Property(x => x.Password).IsModified = false;
    //        await Db.SaveChangesAsync();
    //        return user;
    //    }
    //}
}