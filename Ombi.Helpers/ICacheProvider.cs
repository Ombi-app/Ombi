#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ICacheProvider.cs
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

namespace Ombi.Helpers
{
    public interface ICacheProvider
    {
        /// <summary>
        /// Gets the item from the cache, if the item is not present 
        /// then we will get that item and store it in the cache.
        /// </summary>
        /// <typeparam name="T">Type to store in the cache</typeparam>
        /// <param name="key">The key</param>
        /// <param name="itemCallback">The item callback. This will be called if the item is not present in the cache. </param>
        /// <param name="cacheTime">The amount of time we want to cache the object</param>
        /// <returns><see cref="T"/></returns>
        T GetOrSet<T>(string key, Func<T> itemCallback, int cacheTime = 20) where T : class;
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> itemCallback, int cacheTime = 20) where T : class;

        /// <summary>
        /// Gets the specified item from the cache.
        /// </summary>
        /// <typeparam name="T">Type to get from the cache</typeparam>
        /// <param name="key">The key.</param>
        /// <returns><see cref="T"/></returns>
        T Get<T>(string key) where T : class;

        /// <summary>
        /// Set/Store the specified object in the cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The object we want to store.</param>
        /// <param name="cacheTime">The amount of time we want to cache the object.</param>
        void Set(string key, object data, int cacheTime = 20);

        /// <summary>
        /// Removes the specified object from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        void Remove(string key);
    }
}