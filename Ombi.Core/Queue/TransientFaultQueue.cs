#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: TransientFaultQueue.cs
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
using Dapper;
using Ombi.Helpers;
using Ombi.Store;
using Ombi.Store.Models;
using Ombi.Store.Repository;

namespace Ombi.Core.Queue
{
    public class TransientFaultQueue : ITransientFaultQueue
    {
        public TransientFaultQueue(IRepository<RequestQueue> queue)
        {
            RequestQueue = queue;
        }

        private IRepository<RequestQueue> RequestQueue { get; }


        public void QueueItem(RequestedModel request, string id, RequestType type, FaultType faultType)
        {
            //Ensure there is not a duplicate queued item
            var existingItem = RequestQueue.Custom(
               connection =>
               {
                   connection.Open();
                   var result = connection.Query<RequestQueue>("select * from RequestQueue where PrimaryIdentifier = @ProviderId", new { ProviderId = id });

                   return result;
               }).FirstOrDefault();

            if (existingItem != null)
            {
                // It's already in the queue
                return;
            }
            
            var queue = new RequestQueue
            {
                Type = type,
                Content =  ByteConverterHelper.ReturnBytes(request),
                PrimaryIdentifier = id,
                FaultType = faultType
            };
            RequestQueue.Insert(queue);
        }

        public async Task QueueItemAsync(RequestedModel request, string id, RequestType type, FaultType faultType, string description = null)
        {
            //Ensure there is not a duplicate queued item
            var existingItem = await RequestQueue.CustomAsync(async connection =>
               {
                   connection.Open();
                   var result = await connection.QueryAsync<RequestQueue>("select * from RequestFaultQueue where PrimaryIdentifier = @ProviderId", new { ProviderId = id });

                   return result;
               });

            if (existingItem.FirstOrDefault() != null)
            {
                // It's already in the queue
                return;
            }

            var queue = new RequestQueue
            {
                Type = type,
                Content = ByteConverterHelper.ReturnBytes(request),
                PrimaryIdentifier = id,
                FaultType = faultType,
                Message = description ?? string.Empty
            };
            await RequestQueue.InsertAsync(queue);
        }

        public IEnumerable<RequestQueue> GetQueue()
        {
            var items = RequestQueue.GetAll();


            return items;
        }

        public async Task<IEnumerable<RequestQueue>> GetQueueAsync()
        {
            var items = RequestQueue.GetAllAsync();
            
            return await items;
        }

        public void Dequeue()
        {
            RequestQueue.DeleteAll("RequestQueue");
        }

        public async Task DequeueAsync()
        {
            await RequestQueue.DeleteAllAsync("RequestQueue");
        }
    }
}