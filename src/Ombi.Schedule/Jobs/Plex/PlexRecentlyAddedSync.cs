using System;
using System.Threading.Tasks;
using Ombi.Api.Plex;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexRecentlyAddedSync : IPlexRecentlyAddedSync
    {
        public PlexRecentlyAddedSync(IPlexContentSync contentSync)
        {
            _sync = contentSync;
        }

        private readonly IPlexContentSync _sync;

        public async Task Start()
        {
            await _sync.CacheContent(true);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _sync?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}