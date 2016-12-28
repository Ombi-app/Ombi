#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: IRepository.cs
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
using System.Threading.Tasks;

namespace Ombi.Store.Repository
{
    public interface IRepository<T>
    {
        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        long Insert(T entity);
        Task<int> InsertAsync(T entity);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();


        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        T Get(string id);
        Task<T> GetAsync(string id);
        T Get(int id);
        Task<T> GetAsync(int id);
        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Delete(T entity);
        Task DeleteAsync(T entity);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        bool Update(T entity);
        Task<bool> UpdateAsync(T entity);

        /// <summary>
        /// Updates all.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        bool UpdateAll(IEnumerable<T> entity);
        Task<bool> UpdateAllAsync(IEnumerable<T> entity);

        bool BatchInsert(IEnumerable<T> entities, string tableName, params string[] values);

        IEnumerable<T> Custom(Func<IDbConnection, IEnumerable<T>> func);
        Task<IEnumerable<T>> CustomAsync(Func<IDbConnection, Task<IEnumerable<T>>> func);
        void DeleteAll(string tableName);
        Task DeleteAllAsync(string tableName);

        T Custom(Func<IDbConnection, T> func);
        Task<T> CustomAsync(Func<IDbConnection, Task<T>> func);
    }
}
