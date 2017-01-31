using System.Collections.Generic;
using Ombi.Core.SettingModels;
using Ombi.Store.Models;
using Quartz;

namespace Ombi.Services.Jobs
{
    public interface IUserRequestLimitResetter
    {
        void AlbumLimit(PlexRequestSettings s, IEnumerable<RequestLimit> allUsers);
        void Execute(IJobExecutionContext context);
        void MovieLimit(PlexRequestSettings s, IEnumerable<RequestLimit> allUsers);
        void Start();
        void TvLimit(PlexRequestSettings s, IEnumerable<RequestLimit> allUsers);
    }
}