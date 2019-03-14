using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Ombi.Helpers
{
    public static class GlobalMutex
    {
        public static async Task<T> Lock<T>(Func<Task<T>> func)
        {
            const string mutexId = "Global\\OMBI";
            using (var mutex = new Mutex(false, mutexId, out _))
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

                    return await func();
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
