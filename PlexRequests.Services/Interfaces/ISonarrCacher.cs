using System.Collections.Generic;
using Ombi.Services.Models;

namespace Ombi.Services.Interfaces
{
    public interface ISonarrCacher
    {
        void Queued();
        IEnumerable<SonarrCachedResult> QueuedIds();
    }
}
