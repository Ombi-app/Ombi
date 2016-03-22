#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
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
using System;
using System.Collections.Generic;
using System.Linq;

using Dapper.Contrib.Extensions;

namespace PlexRequests.Store
{
    public class UserRepository<T> : IRepository<T> where T : UserEntity
    {
        public UserRepository(ISqliteConfiguration config)
        {
            Config = config;
        }

        private ISqliteConfiguration Config { get; }
        public long Insert(T entity)
        {
            using (var cnn = Config.DbConnection())
            {
                cnn.Open();
                return cnn.Insert(entity);
            }
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

        public T Get(string id)
        {
            using (var db = Config.DbConnection())
            {
                db.Open();
                var result = db.GetAll<T>();
                var selected = result.FirstOrDefault(x => x.UserGuid == id);
                return selected;
            }
        }

        public T Get(int id)
        {
            throw new NotSupportedException("Get(int) is not supported. Use Get(string)");
        }

        public void Delete(T entity)
        {
            using (var db = Config.DbConnection())
            {
                db.Open();
                db.Delete(entity);
            }
        }

        public bool Update(T entity)
        {
            using (var db = Config.DbConnection())
            {
                db.Open();
                return db.Update(entity);
            }
        }

        public bool UpdateAll(IEnumerable<T> entity)
        {
            throw new NotSupportedException();
        }
    }
}

