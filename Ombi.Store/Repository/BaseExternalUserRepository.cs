#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: UserRepository.cs
//    Created By:
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
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Ombi.Helpers;

namespace Ombi.Store.Repository
{
    public class BaseExternalUserRepository<T> : BaseGenericRepository<T>, IExternalUserRepository<T> where T : Entity
    {
        public BaseExternalUserRepository(ISqliteConfiguration config, ICacheProvider cache) : base(config,cache)
        {
            DbConfig = config;
        }

        private ISqliteConfiguration DbConfig { get; }
        private IDbConnection Db => DbConfig.DbConnection();

        public T GetUser(string userGuid)
        {
            var sql = @"SELECT * FROM PlexUsers
            WHERE PlexUserId = @UserGuid
             COLLATE NOCASE";
            return Db.QueryFirstOrDefault<T>(sql, new {UserGuid = userGuid});
        }

        public T GetUserByUsername(string username)
        {
            var sql = @"SELECT * FROM PlexUsers
            WHERE Username = @UserName
             COLLATE NOCASE";
            return Db.QueryFirstOrDefault<T>(sql, new {UserName = username});
        }

        public async Task<T> GetUserAsync(string userguid)
        {
            var sql = @"SELECT * FROM PlexUsers
            WHERE PlexUserId = @UserGuid
             COLLATE NOCASE";
            return await Db.QueryFirstOrDefaultAsync<T>(sql, new {UserGuid = userguid});
        }

        #region abstract implementation

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        [Obsolete]
        public override T Get(string id)
        {
            throw new System.NotImplementedException();
        }

        [Obsolete]
        public override Task<T> GetAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        [Obsolete]
        public override T Get(int id)
        {
            throw new System.NotImplementedException();
        }

        [Obsolete]
        public override Task<T> GetAsync(string id)
        {
            throw new System.NotImplementedException();
        }

#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        #endregion
    }
}

