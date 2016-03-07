using FluentScheduler;
using Nancy.TinyIoc;

using PlexRequests.Services;

namespace PlexRequests.UI.Jobs
{
    public class PlexTaskFactory : ITaskFactory
    {
        public ITask GetTaskInstance<T>() where T : ITask
        {
            //typeof(AvailabilityUpdateService);
            var container = TinyIoCContainer.Current;

            var a= container.ResolveAll(typeof(T));

            object outT;
            container.TryResolve(typeof(T), out outT);

            return (T)outT;
        }
    }
}