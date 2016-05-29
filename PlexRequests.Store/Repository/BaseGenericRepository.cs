#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: BaseGenericRepository.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using NLog;
using PlexRequests.Helpers;

namespace PlexRequests.Store.Repository
{
    public abstract class BaseGenericRepository<T> where T : class 
    {
        protected BaseGenericRepository(ISqliteConfiguration config, ICacheProvider cache)
        {
            Config = config;
            Cache = cache;
        }
        protected ICacheProvider Cache { get; }
        protected ISqliteConfiguration Config { get; }
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public abstract T Get(string id);
        public abstract Task<T> GetAsync(int id);
        public abstract T Get(int id);
        public abstract Task<T> GetAsync(string id);

        public long Insert(T entity)
        {
            ResetCache();
            using (var cnn = Config.DbConnection())
            {
                cnn.Open();
                return cnn.Insert(entity);
            }
        }

        public void Delete(T entity)
        {
            ResetCache();
            using (var db = Config.DbConnection())
            {
                db.Open();
                db.Delete(entity);
            }
        }

        public async Task DeleteAsync(T entity)
        {
            ResetCache();
            using (var db = Config.DbConnection())
            {
                db.Open();
                await db.DeleteAsync(entity);
            }
        }

        public bool Update(T entity)
        {
            ResetCache();
            Log.Trace("Updating entity");
            Log.Trace(entity.DumpJson());
            using (var db = Config.DbConnection())
            {
                db.Open();
                return db.Update(entity);
            }
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            ResetCache();
            Log.Trace("Updating entity");
            Log.Trace(entity.DumpJson());
            using (var db = Config.DbConnection())
            {
                db.Open();
                return await db.UpdateAsync(entity);
            }
        }

        public bool UpdateAll(IEnumerable<T> entity)
        {
            ResetCache();
            Log.Trace("Updating all entities");
            var result = new HashSet<bool>();

            using (var db = Config.DbConnection())
            {
                db.Open();
                foreach (var e in entity)
                {
                    result.Add(db.Update(e));
                }
            }
            return result.All(x => true);
        }

        public async Task<bool> UpdateAllAsync(IEnumerable<T> entity)
        {
            ResetCache();
            Log.Trace("Updating all entities");
            var result = new HashSet<bool>();

            using (var db = Config.DbConnection())
            {
                db.Open();
                foreach (var e in entity)
                {
                    result.Add(await db.UpdateAsync(e));
                }
            }
            return result.All(x => true);
        }

        public async Task<int> InsertAsync(T entity)
        {
            ResetCache();
            using (var cnn = Config.DbConnection())
            {
                cnn.Open();
                return await cnn.InsertAsync(entity);
            }
        }

        private void ResetCache()
        {
            Cache.Remove("Get");
            Cache.Remove("GetAll");
        }
        public IEnumerable<T> GetAll()
        {

            using (var db = Config.DbConnection())
            {
                db.Open();
                var result = db.GetAll<T>();
                return result;
            }

        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {

            using (var db = Config.DbConnection())
            {
                db.Open();
                var result = await db.GetAllAsync<T>();
                return result;
            }

        }
    }
}