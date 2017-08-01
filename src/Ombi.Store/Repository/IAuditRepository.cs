using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IAuditRepository
    {
        Task Record(AuditType type, AuditArea area, string description);
        Task Record(AuditType type, AuditArea area, string description, string user);
    }
}