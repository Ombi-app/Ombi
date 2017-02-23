using System.Collections.Generic;
using Ombi.Core.SettingModels;
using Ombi.Store;
using Quartz;

namespace Ombi.Services.Jobs
{
    public interface IFaultQueueHandler
    {
        void Execute(IJobExecutionContext context);
        bool ShouldAutoApprove(RequestType requestType, PlexRequestSettings prSettings, List<string> username);
        void Start();
    }
}