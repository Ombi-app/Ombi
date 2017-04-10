using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class RequestJsonRepository : IRequestRepository
    {
        //private ICacheProvider Cache { get; }

        public RequestJsonRepository(IOmbiContext ctx)
        {
            Db = ctx;
        }

        private IOmbiContext Db { get; }

        public RequestBlobs Insert(RequestBlobs entity)
        {

            var id = Db.Requests.Add(entity);
            Db.SaveChanges();
            return id.Entity;

        }

        public async Task<RequestBlobs> InsertAsync(RequestBlobs entity)
        {

            var id = await Db.Requests.AddAsync(entity).ConfigureAwait(false);
            await Db.SaveChangesAsync();
            return id.Entity;

        }

        public IEnumerable<RequestBlobs> GetAll()
        {
            //var key = "GetAll";
            //var item = Cache.GetOrSet(key, () =>
            //{

            var page = Db.Requests.ToList();
            return page;

            //}, 5);
            //return item;
        }

        public async Task<IEnumerable<RequestBlobs>> GetAllAsync()
        {
            //var key = "GetAll";
            //var item = await Cache.GetOrSetAsync(key, async () =>
            //{

            var page = await Db.Requests.ToListAsync().ConfigureAwait(false);
            return page;

            //}, 5);
            //return item;
        }
        public async Task<IEnumerable<RequestBlobs>> GetAllAsync(int count, int position)
        {
            //var key = "GetAll";
            //var item = await Cache.GetOrSetAsync(key, async () =>
            //{

            var page = await Db.Requests.ToListAsync().ConfigureAwait(false);
            return page.Skip(position).Take(count);

            //}, 5);
            //return item;
        }

        public RequestBlobs Get(int id)
        {
            //var key = "Get" + id;
            //var item = Cache.GetOrSet(key, () =>
            //{

            var page = Db.Requests.Find(id);
            return page;

            //}, 5);
            //return item;
        }

        public async Task<RequestBlobs> GetAsync(int id)
        {
            //var key = "Get" + id;
            //var item = await Cache.GetOrSetAsync(key, async () =>
            //{

            var page = await Db.Requests.FindAsync(id).ConfigureAwait(false);
            return page;

            //}, 5);
            //return item;
        }

        public void Delete(RequestBlobs entity)
        {
            //ResetCache();

            Db.Requests.Remove(entity);
            Db.SaveChanges();

        }

        public RequestBlobs Update(RequestBlobs entity)
        {
            try
            {
                Db.SaveChanges();

                return entity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public void UpdateAll(IEnumerable<RequestBlobs> entity)
        {

            Db.Requests.UpdateRange(entity);
            Db.SaveChanges();

        }



        public void DeleteAll(IEnumerable<RequestBlobs> entity)
        {

            Db.Requests.RemoveRange(entity);
            Db.SaveChanges();

        }
    }
}