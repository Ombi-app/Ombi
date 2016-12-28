#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ISettingsRepository.cs
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
using System.Threading.Tasks;
using Ombi.Store.Models;

namespace Ombi.Store.Repository
{
    public interface IRequestRepository
    {
        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        long Insert(RequestBlobs entity);

        Task<int> InsertAsync(RequestBlobs entity);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        IEnumerable<RequestBlobs> GetAll();

        Task<IEnumerable<RequestBlobs>> GetAllAsync();

        RequestBlobs Get(int id);
        Task<RequestBlobs> GetAsync(int id);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        bool Delete(RequestBlobs entity);
        Task<bool> DeleteAsync(RequestBlobs entity);

        bool DeleteAll(IEnumerable<RequestBlobs> entity);
        Task<bool> DeleteAllAsync(IEnumerable<RequestBlobs> entity);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        bool Update(RequestBlobs entity);
        Task<bool> UpdateAsync(RequestBlobs entity);

        bool UpdateAll(IEnumerable<RequestBlobs> entity);
        Task<bool> UpdateAllAsync(IEnumerable<RequestBlobs> entity);
    }
}