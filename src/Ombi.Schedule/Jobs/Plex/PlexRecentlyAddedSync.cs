//using System;
//using System.Threading.Tasks;
//using Hangfire;

//namespace Ombi.Schedule.Jobs.Plex
//{
//    public class PlexRecentlyAddedSync : IPlexRecentlyAddedSync
//    {
//        public PlexRecentlyAddedSync(IPlexContentSync sync)
//        {
//            _sync = sync;
//        }

//        private readonly IPlexContentSync _sync;

//        public void Start()
//        {
//            BackgroundJob.Enqueue(() => _sync.CacheContent(true));
//        }

//        private bool _disposed;
//        protected virtual void Dispose(bool disposing)
//        {
//            if (_disposed)
//                return;

//            if (disposing)
//            {
//                _sync?.Dispose();
//            }
//            _disposed = true;
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}