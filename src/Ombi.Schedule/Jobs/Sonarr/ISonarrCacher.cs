﻿using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Sonarr
{
    public interface ISonarrCacher
    {
        Task Start();
    }
}