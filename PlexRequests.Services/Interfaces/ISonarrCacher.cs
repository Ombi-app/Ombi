using System.Collections.Generic;
using PlexRequests.Services.Models;

namespace PlexRequests.Services.Interfaces
{
    public interface ISonarrCacher
    {
        void Queued();
        IEnumerable<SonarrCachedResult> QueuedIds();
    }
}
