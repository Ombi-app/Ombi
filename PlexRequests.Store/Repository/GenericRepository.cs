#region Copyright

// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: GenericRepository.cs
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
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Ombi.Helpers;

namespace Ombi.Store.Repository
{
    public class GenericRepository<T> : BaseGenericRepository<T>, IRepository<T> where T : Entity
    {
 
        public GenericRepository(ISqliteConfiguration config, ICacheProvider cache) : base(config, cache)
        {
           
        }
        
        public override T Get(string id)
        {
            throw new NotSupportedException("Get(string) is not supported. Use Get(int)");
        }

        public override Task<T> GetAsync(string id)
        {
            throw new NotSupportedException("GetAsync(string) is not supported. Use GetAsync(int)");
        }

        public override T Get(int id)
        {
            var key = "Get" + id;
            var item = Cache.GetOrSet(
                key,
                () =>
                {
                    using (var db = Config.DbConnection())
                    {
                        db.Open();
                        return db.Get<T>(id);
                    }
                });
            return item;
        }

        public override async Task<T> GetAsync(int id)
        {
            var key = "Get" + id;
            var item = await Cache.GetOrSetAsync(
                key,
                async () =>
                {
                    using (var db = Config.DbConnection())
                    {
                        db.Open();
                        return await db.GetAsync<T>(id).ConfigureAwait(false);
                    }
                });
            return item;
        }

        
    }
}