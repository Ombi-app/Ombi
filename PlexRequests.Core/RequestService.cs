#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RequestService.cs
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
using PlexRequests.Store;

namespace PlexRequests.Core
{
    public class RequestService : IRequestService
    {
        public RequestService(IRepository<RequestedModel> db)
        {
            Repo = db;
        }
    
        private IRepository<RequestedModel> Repo { get; set; }

        public long AddRequest(int providerId, RequestedModel model)
        {
            return Repo.Insert(model);
        }

        public bool CheckRequest(int providerId)
        {
            return Repo.GetAll().Any(x => x.ProviderId == providerId);
        }

        public void DeleteRequest(RequestedModel model)
        {
            var entity = Repo.Get(model.Id);
            Repo.Delete(entity);
        }

        public bool UpdateRequest(RequestedModel model)
        {
            return Repo.Update(model);
        }

        /// <summary>
        /// Updates all the entities. NOTE: we need to Id to be the original entity
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public bool BatchUpdate(List<RequestedModel> model)
        {
           return Repo.UpdateAll(model);
        }

        public RequestedModel Get(int id)
        {
            return Repo.Get(id);
        }

        public IEnumerable<RequestedModel> GetAll()
        {
            return Repo.GetAll();
        }
    }
}
