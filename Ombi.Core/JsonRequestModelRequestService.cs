#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: JsonRequestModelRequestService.cs
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
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ombi.Helpers;
using Ombi.Store;
using Ombi.Store.Models;
using Ombi.Store.Repository;

namespace Ombi.Core
{
    public class JsonRequestModelRequestService : IRequestService
    {
        public JsonRequestModelRequestService(IRequestRepository repo)
        {
            Repo = repo;
        }
        private IRequestRepository Repo { get; }
        public long AddRequest(RequestedModel model)
        {
            var entity = new RequestBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), ProviderId = model.ProviderId };
            var id = Repo.Insert(entity);

            model.Id = (int)id;

            entity = new RequestBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), ProviderId = model.ProviderId, Id = (int)id, MusicId = model.MusicBrainzId };
            var result = Repo.Update(entity);

            return result ? id : -1;
        }

        public async Task<int> AddRequestAsync(RequestedModel model)
        {
            var entity = new RequestBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), ProviderId = model.ProviderId };
            var id = await Repo.InsertAsync(entity).ConfigureAwait(false);

            model.Id = id;

            entity = new RequestBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), ProviderId = model.ProviderId, Id = id, MusicId = model.MusicBrainzId };
            var result = await Repo.UpdateAsync(entity).ConfigureAwait(false);

            return result ? id : -1;
        }

        public RequestedModel CheckRequest(int providerId)
        {
            var blobs = Repo.GetAll();
            var blob = blobs.FirstOrDefault(x => x.ProviderId == providerId);
            return blob != null ? ByteConverterHelper.ReturnObject<RequestedModel>(blob.Content) : null;
        }

        public async Task<RequestedModel> CheckRequestAsync(int providerId)
        {
            var blobs = await Repo.GetAllAsync().ConfigureAwait(false);
            var blob = blobs.FirstOrDefault(x => x.ProviderId == providerId);
            return blob != null ? ByteConverterHelper.ReturnObject<RequestedModel>(blob.Content) : null;
        }

        public RequestedModel CheckRequest(string musicId)
        {
            var blobs = Repo.GetAll();
            var blob = blobs.FirstOrDefault(x => x.MusicId == musicId);
            return blob != null ? ByteConverterHelper.ReturnObject<RequestedModel>(blob.Content) : null;
        }

        public async Task<RequestedModel> CheckRequestAsync(string musicId)
        {
            var blobs = await Repo.GetAllAsync().ConfigureAwait(false);
            var blob = blobs.FirstOrDefault(x => x.MusicId == musicId);
            return blob != null ? ByteConverterHelper.ReturnObject<RequestedModel>(blob.Content) : null;
        }

        public void DeleteRequest(RequestedModel request)
        {
            var blob = Repo.Get(request.Id);
            Repo.Delete(blob);
        }

        public async Task DeleteRequestAsync(RequestedModel request)
        {
            var blob = await Repo.GetAsync(request.Id).ConfigureAwait(false);
            await Repo.DeleteAsync(blob).ConfigureAwait(false);
        }

        public bool UpdateRequest(RequestedModel model)
        {
            var entity = new RequestBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), ProviderId = model.ProviderId, Id = model.Id };
            return Repo.Update(entity);
        }

        public async Task<bool> UpdateRequestAsync(RequestedModel model)
        {
            var entity = new RequestBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), ProviderId = model.ProviderId, Id = model.Id };
            return await Repo.UpdateAsync(entity).ConfigureAwait(false);
        }

        public RequestedModel Get(int id)
        {
            var blob = Repo.Get(id);
            if (blob == null)
            {
                return new RequestedModel();
            }
            var model = ByteConverterHelper.ReturnObject<RequestedModel>(blob.Content);
            return model;
        }

        public async Task<RequestedModel> GetAsync(int id)
        {
            var blob = await Repo.GetAsync(id).ConfigureAwait(false);
            if (blob == null)
            {
                return new RequestedModel();
            }
            var model = ByteConverterHelper.ReturnObject<RequestedModel>(blob.Content);
            return model;
        }

        public IEnumerable<RequestedModel> GetAll()
        {
            var blobs = Repo.GetAll();
            return blobs.Select(b => Encoding.UTF8.GetString(b.Content))
                .Select(JsonConvert.DeserializeObject<RequestedModel>)
                .ToList();
        }

        public async Task<IEnumerable<RequestedModel>> GetAllAsync()
        {
            var blobs = await Repo.GetAllAsync().ConfigureAwait(false);
            return blobs.Select(b => Encoding.UTF8.GetString(b.Content))
                .Select(JsonConvert.DeserializeObject<RequestedModel>)
                .ToList();
        }

        public bool BatchUpdate(IEnumerable<RequestedModel> model)
        {
            var entities = model.Select(m => new RequestBlobs { Type = m.Type, Content = ByteConverterHelper.ReturnBytes(m), ProviderId = m.ProviderId, Id = m.Id }).ToList();
            return Repo.UpdateAll(entities);
        }
        public async Task<bool> BatchUpdateAsync(IEnumerable<RequestedModel> model)
        {
            var entities = model.Select(m => new RequestBlobs { Type = m.Type, Content = ByteConverterHelper.ReturnBytes(m), ProviderId = m.ProviderId, Id = m.Id }).ToList();
            return await Repo.UpdateAllAsync(entities).ConfigureAwait(false);
        }
        public bool BatchDelete(IEnumerable<RequestedModel> model)
        {
            var entities = model.Select(m => new RequestBlobs { Type = m.Type, Content = ByteConverterHelper.ReturnBytes(m), ProviderId = m.ProviderId, Id = m.Id }).ToList();
            return Repo.DeleteAll(entities);
        }

        public async Task<bool> BatchDeleteAsync(IEnumerable<RequestedModel> model)
        {
            var entities = model.Select(m => new RequestBlobs { Type = m.Type, Content = ByteConverterHelper.ReturnBytes(m), ProviderId = m.ProviderId, Id = m.Id }).ToList();
            return await Repo.DeleteAllAsync(entities).ConfigureAwait(false);
        }
    }
}