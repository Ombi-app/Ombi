using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Requests.Models;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Models.Requests
{
    public class JsonRequestService<T> : IRequestService<T> where T:BaseRequestModel
    {
        public JsonRequestService(IRequestRepository repo)
        {
            Repo = repo;
        }
        private IRequestRepository Repo { get; }
        public int AddRequest(T model)
        {
            var entity = new RequestBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), ProviderId = model.ProviderId };
            var id = Repo.Insert(entity);

            return id.Id;
        }

        public async Task<int> AddRequestAsync(T model)
        {
            var entity = new RequestBlobs { Type = model.Type, Content = ByteConverterHelper.ReturnBytes(model), ProviderId = model.ProviderId };
            var id = await Repo.InsertAsync(entity).ConfigureAwait(false);

        return id.Id;
        }

        public T CheckRequest(int providerId)
        {
            var blobs = Repo.GetAll();
            var blob = blobs.FirstOrDefault(x => x.ProviderId == providerId); if (blob == null)
            {
                return null;
            }
            var model = ByteConverterHelper.ReturnObject<T>(blob.Content);
            model.Id = blob.Id;
            return model;
        }

        public async Task<T> CheckRequestAsync(int providerId)
        {
            var blobs = await Repo.GetAllAsync().ConfigureAwait(false);
            var blob = blobs.FirstOrDefault(x => x.ProviderId == providerId); if (blob == null)
            {
                return null;
            }
            var model = ByteConverterHelper.ReturnObject<T>(blob.Content);
            model.Id = blob.Id;
            return model;
        }

        public T CheckRequest(string musicId)
        {
            var blobs = Repo.GetAll();
            var blob = blobs.FirstOrDefault(x => x.MusicId == musicId); if (blob == null)
            {
                return null;
            }
            var model = ByteConverterHelper.ReturnObject<T>(blob.Content);
            model.Id = blob.Id;
            return model;
        }

        public async Task<T> CheckRequestAsync(string musicId)
        {
            var blobs = await Repo.GetAllAsync().ConfigureAwait(false);
            var blob = blobs.FirstOrDefault(x => x.MusicId == musicId);

            if (blob == null)
            {
                return null;
            }
            var model = ByteConverterHelper.ReturnObject<T>(blob.Content);
            model.Id = blob.Id;
            return model;
        }

        public void DeleteRequest(T request)
        {
            var blob = Repo.Get(request.Id);
            Repo.Delete(blob);
        }

        public async Task DeleteRequestAsync(T request)
        {
            var blob = await Repo.GetAsync(request.Id).ConfigureAwait(false);
            Repo.Delete(blob);
        }
        public async Task DeleteRequestAsync(int request)
        {
            var blob = await Repo.GetAsync(request).ConfigureAwait(false);
            Repo.Delete(blob);
        }

        public T UpdateRequest(T model)
        {
            var b = Repo.Get(model.Id);
            b.Content = ByteConverterHelper.ReturnBytes(model);
            var blob = Repo.Update(b);
            return model;

        }

        public T Get(int id)
        {
            var blob = Repo.Get(id);
            if (blob == null)
            {
                return default(T);
            }
            var model = ByteConverterHelper.ReturnObject<T>(blob.Content);
            model.Id = blob.Id; // They should always be the same, but for somereason a user didn't have it in the db https://github.com/tidusjar/Ombi/issues/862#issuecomment-269743847
            return model;
        }

        public async Task<T> GetAsync(int id)
        {
            var blob = await Repo.GetAsync(id).ConfigureAwait(false);
            if (blob == null)
            {
                return default(T);
            }
            var model = ByteConverterHelper.ReturnObject<T>(blob.Content);
            model.Id = blob.Id;
            return model;
        }

        public IEnumerable<T> GetAll()
        {
            var blobs = Repo.GetAll().ToList();
            var retVal = new List<T>();

            foreach (var b in blobs)
            {
                if (b == null)
                {
                    continue;
                }
                var model = ByteConverterHelper.ReturnObject<T>(b.Content);
                model.Id = b.Id;
                retVal.Add(model);
            }
            return retVal;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var blobs = await Repo.GetAllAsync().ConfigureAwait(false);
            var retVal = new List<T>();

            foreach (var b in blobs)
            {
                if (b == null)
                {
                    continue;
                }
                var model = ByteConverterHelper.ReturnObject<T>(b.Content);
                model.Id = b.Id;
                retVal.Add(model);
            }
            return retVal;
        }

        public async Task<IEnumerable<T>> GetAllAsync(int count, int position)
        {
            var blobs = await Repo.GetAllAsync(count, position).ConfigureAwait(false);
            var retVal = new List<T>();

            foreach (var b in blobs)
            {
                if (b == null)
                {
                    continue;
                }
                var model = ByteConverterHelper.ReturnObject<T>(b.Content);
                model.Id = b.Id;
                retVal.Add(model);
            }
            return retVal;
        }

        public void BatchUpdate(IEnumerable<T> model)
        {
            var entities = model.Select(m => new RequestBlobs { Type = m.Type, Content = ByteConverterHelper.ReturnBytes(m), ProviderId = m.ProviderId, Id = m.Id }).ToList();
            Repo.UpdateAll(entities);
        }

        public void BatchDelete(IEnumerable<T> model)
        {
            var entities = model.Select(m => new RequestBlobs { Type = m.Type, Content = ByteConverterHelper.ReturnBytes(m), ProviderId = m.ProviderId, Id = m.Id }).ToList();
            Repo.DeleteAll(entities);
        }
    }
}