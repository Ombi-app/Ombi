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
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Mono.Data.Sqlite;
using NLog;
using Ombi.Helpers;

namespace Ombi.Store.Repository
{
    public abstract class BaseGenericRepository<T> where T : class
    {
        private const string CorruptMessage =
            "The database is corrupt, this could be due to the application exiting unexpectedly. See here to fix it: http://www.dosomethinghere.com/2013/02/20/fixing-the-sqlite-error-the-database-disk-image-is-malformed/";
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
        private IDbConnection Connection => Config.DbConnection();


        public IEnumerable<T> Custom(Func<IDbConnection, IEnumerable<T>> func)
        {
            using (var cnn = Connection)
            {
                return func(cnn);
            }
        }

        public T Custom(Func<IDbConnection, T> func)
        {
            using (var cnn = Connection)
            {
                return func(cnn);
            }
        }

        public async Task<IEnumerable<T>> CustomAsync(Func<IDbConnection, Task<IEnumerable<T>>> func)
        {
            using (var cnn = Connection)
            {
                return await func(cnn);
            }
        }
        public async Task<T> CustomAsync(Func<IDbConnection, Task<T>> func)
        {
            using (var cnn = Connection)
            {
                return await func(cnn);
            }
        }

        public long Insert(T entity)
        {
            try
            {
                ResetCache();
                using (var cnn = Config.DbConnection())
                {
                    cnn.Open();
                    return cnn.Insert(entity);
                }
            }
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }
        }

        public void Delete(T entity)
        {
            try
            {
                ResetCache();
                using (var db = Config.DbConnection())
                {
                    db.Open();
                    db.Delete(entity);
                }
            }
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }
        }

        public async Task DeleteAsync(T entity)
        {
            try
            {
                ResetCache();
                using (var db = Config.DbConnection())
                {
                    db.Open();
                    await db.DeleteAsync(entity).ConfigureAwait(false);
                }
            }
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }
        }

        public bool Update(T entity)
        {
            try
            {
                ResetCache();
                using (var db = Config.DbConnection())
                {
                    db.Open();
                    return db.Update(entity);
                }
            }
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                ResetCache();
                using (var db = Config.DbConnection())
                {
                    db.Open();
                    return await db.UpdateAsync(entity).ConfigureAwait(false);
                }
            }
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }
        }

        public bool UpdateAll(IEnumerable<T> entity)
        {
            try
            {
                ResetCache();
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
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }
        }

        public async Task<bool> UpdateAllAsync(IEnumerable<T> entity)
        {
            try
            {
                ResetCache();
                var tasks = new List<Task<bool>>();

                using (var db = Config.DbConnection())
                {
                    db.Open();
                    foreach (var e in entity)
                    {
                        tasks.Add(db.UpdateAsync(e));
                    }
                }
                var result = await Task.WhenAll(tasks).ConfigureAwait(false);
                return result.All(x => true);
            }
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }
        }

        public async Task<int> InsertAsync(T entity)
        {
            try
            {
                ResetCache();
                using (var cnn = Config.DbConnection())
                {
                    cnn.Open();
                    return await cnn.InsertAsync(entity).ConfigureAwait(false);
                }
            }
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }
        }

        private void ResetCache()
        {
            Cache.Remove("Get");
            Cache.Remove("GetAll");
        }
        public IEnumerable<T> GetAll()
        {
            try
            {
                using (var db = Config.DbConnection())
                {
                    db.Open();
                    var result = db.GetAll<T>();
                    return result;
                }
            }
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }

        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                using (var db = Config.DbConnection())
                {
                    db.Open();
                    var result = await db.GetAllAsync<T>().ConfigureAwait(false);
                    return result;
                }
            }
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }
        }

        public bool BatchInsert(IEnumerable<T> entities, string tableName, params string[] values)
        {
            // If we have nothing to update, then it didn't fail...
            var enumerable = entities as T[] ?? entities.ToArray();
            if (!enumerable.Any())
            {
                return true;
            }

            try
            {
                ResetCache();
                using (var db = Config.DbConnection())
                {
                    db.Open();
                    using (var tran = db.BeginTransaction())
                    {
                        var result = enumerable.Sum(e => db.Insert(e));
                        if (result != 0)
                        {
                            tran.Commit();
                            return true;
                        }
                        tran.Rollback();
                        return false;
                    }
                }

            }
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }
        }


        public void DeleteAll(string tableName)
        {
            try
            {
                ResetCache();
                using (var db = Config.DbConnection())
                {
                    db.Open();
                    db.Execute($"delete from {tableName}");
                }
            }
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }
        }
        public async Task DeleteAllAsync(string tableName)
        {
            try
            {
                ResetCache();
                using (var db = Config.DbConnection())
                {
                    db.Open();
                    await db.ExecuteAsync($"delete from {tableName}");
                }
            }
            catch (SqliteException e) when (e.ErrorCode == SQLiteErrorCode.Corrupt)
            {
                Log.Fatal(CorruptMessage);
                throw;
            }
        }
    }
}