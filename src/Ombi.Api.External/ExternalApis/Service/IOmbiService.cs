using System.Threading.Tasks;
using Ombi.Api.External.ExternalApis.Service.Models;

namespace Ombi.Api.External.ExternalApis.Service
{
    public interface IOmbiService
    {
        Task<Updates> GetUpdates(string branch);
    }
}