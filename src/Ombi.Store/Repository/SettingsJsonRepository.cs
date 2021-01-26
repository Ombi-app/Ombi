using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ombi.Helpers;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class SettingsJsonRepository : ISettingsRepository
    {
        public SettingsJsonRepository(SettingsContext ctx, ICacheService mem)
        {
            Db = ctx;
            _cache = mem;
        }

        private SettingsContext Db { get; }
        private readonly ICacheService _cache;

        public GlobalSettings Insert(GlobalSettings entity)
        {
            //_cache.Remove(GetName(entity.SettingsName));

            var settings = Db.Settings.Add(entity);
            Db.SaveChanges();
            return settings.Entity;

        }

        public async Task<GlobalSettings> InsertAsync(GlobalSettings entity)
        {

            //_cache.Remove(GetName(entity.SettingsName));
            var settings = await Db.Settings.AddAsync(entity);
            await Db.SaveChangesAsync();

            return settings.Entity;

        }


        public GlobalSettings Get(string pageName)
        {
            //return _cache.GetOrCreate(GetName(pageName), entry =>
            //{
            //    entry.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
            var entity = Db.Settings.AsNoTracking().FirstOrDefault(x => x.SettingsName == pageName);
            return entity;
            //});
        }

        public async Task<GlobalSettings> GetAsync(string settingsName)
        {
            //return await _cache.GetOrCreateAsync(GetName(settingsName), async entry =>
            //{
            //entry.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
            var obj = await Db.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingsName == settingsName);
            return obj;
            //});
        }

        public async Task DeleteAsync(GlobalSettings entity)
        {
            //_cache.Remove(GetName(entity.SettingsName));
            Db.Settings.Remove(entity);
            await InternalSaveChanges();
        }

        public async Task UpdateAsync(GlobalSettings entity)
        {
            //_cache.Remove(GetName(entity.SettingsName));
            Db.Update(entity);
            await InternalSaveChanges();
        }

        public void Delete(GlobalSettings entity)
        {
            //_cache.Remove(GetName(entity.SettingsName));

            Db.Settings.Remove(entity);
            Db.SaveChanges();
        }

        public void Update(GlobalSettings entity)
        {
            Db.Update(entity);
            //_cache.Remove(GetName(entity.SettingsName));
            Db.SaveChanges();
        }

        private string GetName(string entity)
        {
            return $"{entity}Json";
        }

        private async Task<int> InternalSaveChanges()
        {
            var r = await Db.SaveChangesAsync();
            return r;
        }
    }
}