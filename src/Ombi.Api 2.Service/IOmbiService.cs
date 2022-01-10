using System.Threading.Tasks;
using Ombi.Api.Service.Models;

namespace Ombi.Api.Service
{
    public interface IOmbiService
    {
        Task<Updates> GetUpdates(string branch);
    }
}