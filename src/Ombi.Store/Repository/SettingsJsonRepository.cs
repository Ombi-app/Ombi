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

            var page = Db.Settings.ToList();
            return page;

        }

        public async Task<IEnumerable<GlobalSettings>> GetAllAsync()
        {
            var page = await Db.Settings.ToListAsync();
            return page;
        }

        public GlobalSettings Get(string pageName)
        {
            var entity = Db.Settings.FirstOrDefault(x => x.SettingsName == pageName);
            if (entity == null)
            {
                throw new ArgumentNullException($"The setting {pageName} does not exist");
            }
            Db.Entry(entity).Reload();
            return entity;
        }

        public async Task<GlobalSettings> GetAsync(string settingsName)
        {
            return await Db.Settings.FirstOrDefaultAsync(x => x.SettingsName == settingsName);
        }

        public async Task DeleteAsync(GlobalSettings entity)
        {
            Db.Settings.Remove(entity);
            await Db.SaveChangesAsync();
        }

        public async Task UpdateAsync(GlobalSettings entity)
        {
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