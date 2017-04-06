using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Requests.Models;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Models.Requests
{
    public class JsonRequestService : IRequestService
    {
        public JsonRequestService(IRequestRepository repo)
        {
            Repo = repo;
        }
        private IRequestRepository Repo { get; }
        public int AddRequest(RequestModel model)
        {
            var entity = new RequestBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), ProviderId = model.ProviderId };
            var id = Repo.Insert(entity);

            return id.Id;
        }

        public async Task<int> AddRequestAsync(RequestModel model)
        {
            var entity = new RequestBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), ProviderId = model.ProviderId };
            var id = await Repo.InsertAsync(entity).ConfigureAwait(false);

        return id.Id;
        }

        public RequestModel CheckRequest(int providerId)
        {
            var blobs = Repo.GetAll();
            var blob = blobs.FirstOrDefault(x => x.ProviderId == providerId); if (blob == null)
            {
                return null;
            }
            var model = ByteConverterHelper.ReturnObject<RequestModel>(blob.Content);
            model.Id = blob.Id;
            return model;
        }

        public async Task<RequestModel> CheckRequestAsync(int providerId)
        {
            var blobs = await Repo.GetAllAsync().ConfigureAwait(false);
            var blob = blobs.FirstOrDefault(x => x.ProviderId == providerId); if (blob == null)
            {
                return null;
            }
            var model = ByteConverterHelper.ReturnObject<RequestModel>(blob.Content);
            model.Id = blob.Id;
            return model;
        }

        public RequestModel CheckRequest(string musicId)
        {
            var blobs = Repo.GetAll();
            var blob = blobs.FirstOrDefault(x => x.MusicId == musicId); if (blob == null)
            {
                return null;
            }
            var model = ByteConverterHelper.ReturnObject<RequestModel>(blob.Content);
            model.Id = blob.Id;
            return model;
        }

        public async Task<RequestModel> CheckRequestAsync(string musicId)
        {
            var blobs = await Repo.GetAllAsync().ConfigureAwait(false);
            var blob = blobs.FirstOrDefault(x => x.MusicId == musicId);

            if (blob == null)
            {
                return null;
            }
            var model = ByteConverterHelper.ReturnObject<RequestModel>(blob.Content);
            model.Id = blob.Id;
            return model;
        }

        public void DeleteRequest(RequestModel request)
        {
            var blob = Repo.Get(request.Id);
            Repo.Delete(blob);
        }

        public async Task DeleteRequestAsync(RequestModel request)
        {
            var blob = await Repo.GetAsync(request.Id).ConfigureAwait(false);
            Repo.Delete(blob);
        }

        public RequestBlobs UpdateRequest(RequestModel model)
        {
            var entity = new RequestBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), ProviderId = model.ProviderId, Id = model.Id };
            return Repo.Update(entity);
        }

        public RequestModel Get(int id)
        {
            var blob = Repo.Get(id);
            if (blob == null)
            {
                return new RequestModel();
            }
            var model = ByteConverterHelper.ReturnObject<RequestModel>(blob.Content);
            model.Id = blob.Id; // They should always be the same, but for somereason a user didn't have it in the db https://github.com/tidusjar/Ombi/issues/862#issuecomment-269743847
            return model;
        }

        public async Task<RequestModel> GetAsync(int id)
        {
            var blob = await Repo.GetAsync(id).ConfigureAwait(false);
            if (blob == null)
            {
                return new RequestModel();
            }
            var model = ByteConverterHelper.ReturnObject<RequestModel>(blob.Content);
            model.Id = blob.Id;
            return model;
        }

        public IEnumerable<RequestModel> GetAll()
        {
            var blobs = Repo.GetAll().ToList();
            var retVal = new List<RequestModel>();

            foreach (var b in blobs)
            {
                if (b == null)
                {
                    continue;
                }
                var model = ByteConverterHelper.ReturnObject<RequestModel>(b.Content);
                model.Id = b.Id;
                retVal.Add(model);
            }
            return retVal;
        }

        public async Task<IEnumerable<RequestModel>> GetAllAsync()
        {
            var blobs = await Repo.GetAllAsync().ConfigureAwait(false);
            var retVal = new List<RequestModel>();

            foreach (var b in blobs)
            {
                if (b == null)
                {
                    continue;
                }
                var model = ByteConverterHelper.ReturnObject<RequestModel>(b.Content);
                model.Id = b.Id;
                retVal.Add(model);
            }
            return retVal;
        }

        public void BatchUpdate(IEnumerable<RequestModel> model)
        {
            var entities = model.Select(m => new RequestBlobs { Type = m.Type, Content = ByteConverterHelper.ReturnBytes(m), ProviderId = m.ProviderId, Id = m.Id }).ToList();
            Repo.UpdateAll(entities);
        }

        public void BatchDelete(IEnumerable<RequestModel> model)
        {
            var entities = model.Select(m => new RequestBlobs { Type = m.Type, Content = ByteConverterHelper.ReturnBytes(m), ProviderId = m.ProviderId, Id = m.Id }).ToList();
            Repo.DeleteAll(entities);
        }
    }
}