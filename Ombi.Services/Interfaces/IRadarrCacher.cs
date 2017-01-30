using System.Collections.Generic;
using Ombi.Services.Models;

namespace Ombi.Services.Interfaces
{
    public interface IRadarrCacher
    {
        void Queued();
        int[] QueuedIds();
    }
}
