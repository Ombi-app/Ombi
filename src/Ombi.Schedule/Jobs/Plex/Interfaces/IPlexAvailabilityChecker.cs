﻿using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Plex
{
    public interface IPlexAvailabilityChecker : IBaseJob
    {
        Task Start();
    }
}