using Ombi.Core.SettingModels;
using Quartz;

namespace Ombi.Services.Jobs
{
    public interface IPlexEpisodeCacher
    {
        void CacheEpisodes(PlexSettings settings);
        void Execute(IJobExecutionContext context);
        void Start();
    }
}