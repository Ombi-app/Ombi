using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Helpers;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class NotificationTemplatesRepository : INotificationTemplatesRepository
    {
        public NotificationTemplatesRepository(IOmbiContext ctx)
        {
            Db = ctx;
        }

        private IOmbiContext Db { get; }

        public IQueryable<NotificationTemplates> All()
        {
            return Db.NotificationTemplates.AsQueryable();
        }

        public async Task<IEnumerable<NotificationTemplates>> GetAllTemplates()
        {
            return await Db.NotificationTemplates.ToListAsync();
        }

        public async Task<IEnumerable<NotificationTemplates>> GetAllTemplates(NotificationAgent agent)
        {
            return await Db.NotificationTemplates.Where(x => x.Agent == agent).ToListAsync();
        }

        public async Task<NotificationTemplates> GetTemplate(NotificationAgent agent, NotificationType type)
        {
            return await Db.NotificationTemplates.FirstOrDefaultAsync(x => x.Agent == agent && x.NotificationType == type);
        }

        public async Task Update(NotificationTemplates template)
        {
            await Db.SaveChangesAsync();
        }

        public async Task UpdateRange(IEnumerable<NotificationTemplates> templates)
        {
            foreach (var t in templates)
            {

                Db.Attach(t);
                Db.Entry(t).State = EntityState.Modified;
            }
            await Db.SaveChangesAsync();
        }

        public async Task<NotificationTemplates> Insert(NotificationTemplates entity)
        {
            var settings = await Db.NotificationTemplates.AddAsync(entity).ConfigureAwait(false);
            await Db.SaveChangesAsync().ConfigureAwait(false);
            return settings.Entity;
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Db?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}