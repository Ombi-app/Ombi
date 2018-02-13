using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class ApplicationConfigRepository : IApplicationConfigRepository
    {
        public ApplicationConfigRepository(IOmbiContext ctx)
        {
            Ctx = ctx;
        }

        private IOmbiContext Ctx { get; }

        public async Task<ApplicationConfiguration> Get(ConfigurationTypes type)
        {
            return await Ctx.ApplicationConfigurations.FirstOrDefaultAsync(x => x.Type == type);
        }
    }
}