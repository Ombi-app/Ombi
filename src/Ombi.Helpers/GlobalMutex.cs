using System;
using System.Threading;

namespace Ombi.Helpers
{
    public static class GlobalMutex
    {
        public static T Lock<T>(Func<T> func)
        {
            const string mutexId = "Global\\OMBI";

            using (var mutex = new Mutex(false, mutexId, out var __))
            {
                var hasHandle = false;
                try
                {
                    try
                    {
                        hasHandle = mutex.WaitOne(5000, false);
                        if (hasHandle == false)
                            throw new TimeoutException("Timeout waiting for exclusive access");
                    }
                    catch (AbandonedMutexException)
                    {
                        hasHandle = true;
                    }

                    return func();
                }
                finally
                {
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }
        }
    }
}
