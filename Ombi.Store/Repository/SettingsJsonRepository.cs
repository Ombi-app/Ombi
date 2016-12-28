#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SettingsJsonRepository.cs
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
using Dapper.Contrib.Extensions;
using Ombi.Helpers;
using Ombi.Store.Models;

namespace Ombi.Store.Repository
{
    public class SettingsJsonRepository : ISettingsRepository
    {
        private ICacheProvider Cache { get; }

        private string TypeName { get; }
        public SettingsJsonRepository(ISqliteConfiguration config, ICacheProvider cacheProvider)
        {
            Db = config;
            Cache = cacheProvider;
            TypeName = typeof(SettingsJsonRepository).Name;
        }

        private ISqliteConfiguration Db { get; }

        public long Insert(GlobalSettings entity)
        {
            ResetCache();
            using (var con = Db.DbConnection())
            {
                return con.Insert(entity);
            }
        }

        public async Task<int> InsertAsync(GlobalSettings entity)
        {
            ResetCache();
            using (var con = Db.DbConnection())
            {
                return await con.InsertAsync(entity).ConfigureAwait(false);
            }
        }

        public IEnumerable<GlobalSettings> GetAll()
        {
            var key = TypeName + "GetAll";
            var item = Cache.GetOrSet(key, () =>
            {
                using (var con = Db.DbConnection())
                {
                    var page = con.GetAll<GlobalSettings>();
                    return page;
                }
            }, 5);
            return item;
        }

        public async Task<IEnumerable<GlobalSettings>> GetAllAsync()
        {
            var key = TypeName + "GetAll";
            var item = await Cache.GetOrSetAsync(key, async() =>
            {
                using (var con = Db.DbConnection())
                {
                    var page = await con.GetAllAsync<GlobalSettings>().ConfigureAwait(false);
                    return page;
                }
            }, 5);
            return item;
        }

        public GlobalSettings Get(string pageName)
        {
            var key = pageName + "Get";
            var item = Cache.GetOrSet(key, () =>
            {
                using (var con = Db.DbConnection())
                {
                    var page = con.GetAll<GlobalSettings>().SingleOrDefault(x => x.SettingsName == pageName);
                    return page;
                }
            }, 5);
            return item;
        }

        public async Task<GlobalSettings> GetAsync(string settingsName)
        {
            var key = settingsName + "Get";
            var item = await Cache.GetOrSetAsync(key, async() =>
            {
                using (var con = Db.DbConnection())
                {
                    var page = await con.GetAllAsync<GlobalSettings>().ConfigureAwait(false);
                    return page.SingleOrDefault(x => x.SettingsName == settingsName);
                }
            }, 5);
            return item;
        }

        public async Task<bool> DeleteAsync(GlobalSettings entity)
        {
            ResetCache();
            using (var con = Db.DbConnection())
            {
                return await con.DeleteAsync(entity).ConfigureAwait(false);
            }
        }

        public async Task<bool> UpdateAsync(GlobalSettings entity)
        {
            ResetCache();
            using (var con = Db.DbConnection())
            {
                return await con.UpdateAsync(entity).ConfigureAwait(false);
            }
        }

        public bool Delete(GlobalSettings entity)
        {
            ResetCache();
            using (var con = Db.DbConnection())
            {
                return con.Delete(entity);
            }
        }

        public bool Update(GlobalSettings entity)
        {
            ResetCache();
            using (var con = Db.DbConnection())
            {
                return con.Update(entity);
            }
        }

        private void ResetCache()
        {
            Cache.Remove("Get");
            Cache.Remove(TypeName + "GetAll");
        }
    }
}
