using System.Threading.Tasks;

namespace Ombi.Api.External.ExternalApis.Service
{
    public interface IAppVeyorApi
    {
        Task<AppVeyorApi.AppveyorProjects> GetProjectHistory(string branchName, int records = 10);
    }
}