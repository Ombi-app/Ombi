﻿using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Emby
{
    public interface IEmbyEpisodeSync : IBaseJob
    {
        Task Start();
    }
}