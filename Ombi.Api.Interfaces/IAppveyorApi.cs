using Ombi.Api.Models.Appveyor;

namespace Ombi.Api.Interfaces
{
    public interface IAppveyorApi
    {
        AppveyorProjects GetProjectHistory(string branchName, int records = 10);
    }
}