using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Helpers;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface INotificationTemplatesRepository : IDisposable
    {
        IQueryable<NotificationTemplates> All();
        IQueryable<NotificationTemplates> GetAllTemplates();
        IQueryable<NotificationTemplates> GetAllTemplates(NotificationAgent agent);
        Task<NotificationTemplates> Insert(NotificationTemplates entity);
        Task Update(NotificationTemplates template);
        Task UpdateRange(IEnumerable<NotificationTemplates> template);
        Task<NotificationTemplates> GetTemplate(NotificationAgent agent, NotificationType type);
    }
}