using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class AuditRepository : IAuditRepository
    {
        public AuditRepository(OmbiContext ctx)
        {
            Ctx = ctx;
        }

        private OmbiContext Ctx { get; }


        public async Task Record(AuditType type, AuditArea area, string description)
        {
            await Record(type, area, description, string.Empty);
        }

        public async Task Record(AuditType type, AuditArea area, string description, string user)
        {
            await Ctx.Audit.AddAsync(new Audit
            {
                User = user,
                AuditArea = area,
                AuditType = type,
                DateTime = DateTime.UtcNow,
                Description = description
            });

            await Ctx.SaveChangesAsync();
        }
    }
}
