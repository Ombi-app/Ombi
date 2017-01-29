using Ombi.Core.SettingModels;
using Quartz;

namespace Ombi.Services.Jobs.Interfaces
{
    public interface IEmbyEpisodeCacher
    {
        void CacheEpisodes(EmbySettings settings);
        void Execute(IJobExecutionContext context);
        void Start();
    }
}