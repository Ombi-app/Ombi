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
        public NotificationTemplatesRepository(OmbiContext ctx)
        {
            Db = ctx;
        }

        private OmbiContext Db { get; }

        public IQueryable<NotificationTemplates> All()
        {
            return Db.NotificationTemplates.AsQueryable();
        }

        public IQueryable<NotificationTemplates> GetAllTemplates()
        {
            return Db.NotificationTemplates;
        }

        public IQueryable<NotificationTemplates> GetAllTemplates(NotificationAgent agent)
        {
            return Db.NotificationTemplates.Where(x => x.Agent == agent);
        }

        public async Task<NotificationTemplates> GetTemplate(NotificationAgent agent, NotificationType type)
        {
            return await Db.NotificationTemplates.FirstOrDefaultAsync(x => x.Agent == agent && x.NotificationType == type);
        }

        public async Task Update(NotificationTemplates template)
        {
            if (Db.Entry(template).State == EntityState.Detached)
            {
                Db.Attach(template);
                Db.Entry(template).State = EntityState.Modified;
            }
            await InternalSaveChanges();
        }

        public async Task UpdateRange(IEnumerable<NotificationTemplates> templates)
        {
            foreach (var t in templates)
            {

                Db.Attach(t);
                Db.Entry(t).State = EntityState.Modified;
            }
            await InternalSaveChanges();
        }

        public async Task<NotificationTemplates> Insert(NotificationTemplates entity)
        {
            var settings = await Db.NotificationTemplates.AddAsync(entity).ConfigureAwait(false);
            await InternalSaveChanges().ConfigureAwait(false);
            return settings.Entity;
        }

        private async Task<int> InternalSaveChanges()
        {
            return await Db.SaveChangesAsync();
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