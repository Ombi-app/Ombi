using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IApplicationConfigRepository
    {
        Task<ApplicationConfiguration> GetAsync(ConfigurationTypes type);
        ApplicationConfiguration Get(ConfigurationTypes type);
    }
}