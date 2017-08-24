using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class SettingsJsonRepository : ISettingsRepository
    {
        public SettingsJsonRepository(IOmbiContext ctx)
        {
            Db = ctx;
        }

        private IOmbiContext Db { get; }

        public GlobalSettings Insert(GlobalSettings entity)
        {

            var settings = Db.Settings.Add(entity);
            Db.SaveChanges();
            return settings.Entity;

        }

        public async Task<GlobalSettings> InsertAsync(GlobalSettings entity)
        {

            var settings = await Db.Settings.AddAsync(entity).ConfigureAwait(false);
            await Db.SaveChangesAsync().ConfigureAwait(false);
            return settings.Entity;
        }

        public IEnumerable<GlobalSettings> GetAll()
        {

            var page = Db.Settings.AsNoTracking().ToList();
            return page;

        }

        public async Task<IEnumerable<GlobalSettings>> GetAllAsync()
        {
            var page = await Db.Settings.AsNoTracking().ToListAsync();
            return page;
        }

        public GlobalSettings Get(string pageName)
        {
            var entity = Db.Settings.AsNoTracking().FirstOrDefault(x => x.SettingsName == pageName);
            return entity;
        }

        public async Task<GlobalSettings> GetAsync(string settingsName)
        {
           
            var obj =  await Db.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingsName == settingsName);
            return obj;
        }

        public async Task DeleteAsync(GlobalSettings entity)
        {
            Db.Settings.Remove(entity);
            await Db.SaveChangesAsync();
        }

        public async Task UpdateAsync(GlobalSettings entity)
        {
            Db.Update(entity);
            await Db.SaveChangesAsync();
        }

        public void Delete(GlobalSettings entity)
        {
            Db.Settings.Remove(entity);
            Db.SaveChanges();
        }

        public void Update(GlobalSettings entity)
        {
            Db.SaveChanges();
        }
    }
}