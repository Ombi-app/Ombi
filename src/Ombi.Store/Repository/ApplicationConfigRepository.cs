using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class ApplicationConfigRepository : IApplicationConfigRepository
    {
        public ApplicationConfigRepository(ISettingsContext ctx)
        {
            Ctx = ctx;
        }

        private ISettingsContext Ctx { get; }

        public Task<ApplicationConfiguration> GetAsync(ConfigurationTypes type)
        {
            return Ctx.ApplicationConfigurations.FirstOrDefaultAsync(x => x.Type == type);
        }
        public ApplicationConfiguration Get(ConfigurationTypes type)
        {
            return Ctx.ApplicationConfigurations.FirstOrDefault(x => x.Type == type);
        }
    }
}