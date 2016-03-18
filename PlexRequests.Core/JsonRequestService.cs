#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: JsonRequestService.cs
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
using System.Text;

using Newtonsoft.Json;

using PlexRequests.Store;
using PlexRequests.Store.Models;

namespace PlexRequests.Core
{
    public class JsonRequestService : IRequestService
    {
        public JsonRequestService(IRequestRepository repo)
        {
            Repo = repo;
        }
        private IRequestRepository Repo { get; }
        public long AddRequest(int providerId, RequestedModel model)
        {
            var entity = new RequestBlobs { Type = model.Type, Content = ReturnBytes(model), ProviderId = model.ProviderId };
            var id = Repo.Insert(entity);

            // TODO Keep an eye on this, since we are now doing 2 DB update for 1 single request, inserting and then updating
            model.Id = (int)id;

            entity = new RequestBlobs { Type = model.Type, Content = ReturnBytes(model), ProviderId = model.ProviderId, Id = (int)id };
            var result = Repo.Update(entity);

            return result ? id : -1;
        }

        public bool CheckRequest(int providerId)
        {
            var blobs = Repo.GetAll();
            return blobs.Any(x => x.ProviderId == providerId);
        }

        public void DeleteRequest(RequestedModel request)
        {
            var blob = Repo.Get(request.Id);
            Repo.Delete(blob);
        }

        public bool UpdateRequest(RequestedModel model)
        {
            var entity = new RequestBlobs { Type = model.Type, Content = ReturnBytes(model), ProviderId = model.ProviderId, Id = model.Id };
            return Repo.Update(entity);
        }

        public RequestedModel Get(int id)
        {
            var blob = Repo.Get(id);
            var json = Encoding.UTF8.GetString(blob.Content);
            var model = JsonConvert.DeserializeObject<RequestedModel>(json);
            return model;
        }

        public IEnumerable<RequestedModel> GetAll()
        {
            var blobs = Repo.GetAll();
            return blobs.Select(b => Encoding.UTF8.GetString(b.Content))
                .Select(JsonConvert.DeserializeObject<RequestedModel>)
                .ToList();
        }

        public bool BatchUpdate(List<RequestedModel> model)
        {
            var entities = model.Select(m => new RequestBlobs { Type = m.Type, Content = ReturnBytes(m), ProviderId = m.ProviderId, Id = m.Id }).ToList();
            return Repo.UpdateAll(entities);
        }

        public byte[] ReturnBytes(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(json);

            return bytes;
        }
    }
}